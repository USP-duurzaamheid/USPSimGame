window.uspsim2d5 = window.uspsim2d5 || {};

(function () {
    const BASE_CELL_METERS = 2.5;
    const MIN_DOT_SPACING_PX = 12;
    const DOT_COLOR = 'rgba(24, 44, 37, 0.7)';
    const DOT_HALO_COLOR = 'rgba(255, 255, 255, 0.75)';
    const HOVER_FILL = 'rgba(92, 184, 141, 0.22)';
    const HOVER_STROKE = '#439a71';
    const SELECTED_FILL = 'rgba(255, 214, 10, 0.18)';
    const SELECTED_STROKE = '#182c25';

    const canvas = document.createElement('canvas');
    canvas.style.cssText = 'position:absolute;inset:0;width:100%;height:100%;pointer-events:none';

    const grid = {
        map: null,
        layer: null,
        baseSize: 0,
        selectedCell: null,
        hoverCell: null,
        snapInteraction: null,
        snapDraw: null
    };

    // Web Mercator units stretch with latitude. Rounding the latitude keeps the cell size,
    // and so the cell positions, the same for every player in a session.
    function baseSizeFor(map) {
        const center = map.getView().getCenter();
        const latitude = center ? Math.round(ol.proj.toLonLat(center)[1]) : 52;
        return BASE_CELL_METERS / Math.cos(latitude * Math.PI / 180);
    }

    // Distance between the visible dots: the base cell doubled until the dots are far enough apart.
    function spacingFor(resolution) {
        let spacing = grid.baseSize;
        while (spacing / resolution < MIN_DOT_SPACING_PX) spacing *= 2;
        return spacing;
    }

    function cellAt(coordinate, size) {
        return [Math.floor(coordinate[0] / size) * size, Math.floor(coordinate[1] / size) * size, size];
    }

    function cellCenter(cell) {
        return [cell[0] + cell[2] / 2, cell[1] + cell[2] / 2];
    }

    function nearestDot(coordinate, size) {
        return [Math.round(coordinate[0] / size) * size, Math.round(coordinate[1] / size) * size];
    }

    function sameCell(a, b) {
        return !!a && !!b && a[0] === b[0] && a[1] === b[1] && a[2] === b[2];
    }

    function cellCorners(cell, toPixel) {
        const maxX = cell[0] + cell[2];
        const maxY = cell[1] + cell[2];
        return [toPixel(cell[0], cell[1]), toPixel(maxX, cell[1]), toPixel(maxX, maxY), toPixel(cell[0], maxY)];
    }

    function tracePolygon(ctx, corners) {
        ctx.beginPath();
        ctx.moveTo(corners[0][0], corners[0][1]);
        for (let i = 1; i < corners.length; i++) ctx.lineTo(corners[i][0], corners[i][1]);
        ctx.closePath();
    }

    function drawDots(ctx, frameState, toPixel) {
        const spacing = spacingFor(frameState.viewState.resolution);
        const extent = frameState.extent;
        const startX = Math.floor(extent[0] / spacing);
        const endX = Math.ceil(extent[2] / spacing);
        const startY = Math.floor(extent[1] / spacing);
        const endY = Math.ceil(extent[3] / spacing);

        const dots = [];
        for (let i = startX; i <= endX; i++) {
            for (let j = startY; j <= endY; j++) dots.push(toPixel(i * spacing, j * spacing));
        }

        [[DOT_HALO_COLOR, 2], [DOT_COLOR, 1.1]].forEach(function (pass) {
            ctx.fillStyle = pass[0];
            ctx.beginPath();
            dots.forEach(function (p) {
                ctx.moveTo(p[0] + pass[1], p[1]);
                ctx.arc(p[0], p[1], pass[1], 0, 2 * Math.PI);
            });
            ctx.fill();
        });
    }

    function drawHoverCell(ctx, toPixel) {
        tracePolygon(ctx, cellCorners(grid.hoverCell, toPixel));
        ctx.fillStyle = HOVER_FILL;
        ctx.fill();
        ctx.lineWidth = 2;
        ctx.strokeStyle = HOVER_STROKE;
        ctx.stroke();
    }

    function drawSelectedCell(ctx, toPixel) {
        const corners = cellCorners(grid.selectedCell, toPixel);
        tracePolygon(ctx, corners);
        ctx.fillStyle = SELECTED_FILL;
        ctx.fill();
        ctx.lineWidth = 6;
        ctx.strokeStyle = '#ffffff';
        ctx.stroke();
        ctx.lineWidth = 2.5;
        ctx.strokeStyle = SELECTED_STROKE;
        ctx.stroke();

        corners.forEach(function (p) {
            ctx.fillStyle = '#ffffff';
            ctx.fillRect(p[0] - 4, p[1] - 4, 8, 8);
            ctx.fillStyle = SELECTED_STROKE;
            ctx.fillRect(p[0] - 2.5, p[1] - 2.5, 5, 5);
        });
    }

    function render(frameState) {
        const ratio = frameState.pixelRatio;
        const width = frameState.size[0];
        const height = frameState.size[1];
        if (canvas.width !== Math.round(width * ratio) || canvas.height !== Math.round(height * ratio)) {
            canvas.width = Math.round(width * ratio);
            canvas.height = Math.round(height * ratio);
        }

        const ctx = canvas.getContext('2d');
        ctx.setTransform(ratio, 0, 0, ratio, 0, 0);
        ctx.clearRect(0, 0, width, height);

        const t = frameState.coordinateToPixelTransform;
        const toPixel = function (x, y) {
            return [t[0] * x + t[2] * y + t[4], t[1] * x + t[3] * y + t[5]];
        };

        drawDots(ctx, frameState, toPixel);
        if (grid.hoverCell) drawHoverCell(ctx, toPixel);
        if (grid.selectedCell) drawSelectedCell(ctx, toPixel);
        return canvas;
    }

    function redraw() {
        if (grid.layer) grid.layer.changed();
    }

    function currentSpacing() {
        return spacingFor(grid.map.getView().getResolution());
    }

    function handleMapClick(evt) {
        if (grid.snapInteraction) return;
        const cell = cellAt(evt.coordinate, currentSpacing());
        grid.selectedCell = sameCell(cell, grid.selectedCell) ? null : cell;
        redraw();
    }

    function selectPlacedCell(evt) {
        const point = evt.feature && evt.feature.getGeometry();
        if (point) {
            grid.selectedCell = cellAt(point.getCoordinates(), currentSpacing());
            redraw();
        }
    }

    window.uspsim2d5.attachGrid = function () {
        const map = window.activeOlMap;
        if (!map || grid.map === map) return;

        if (grid.map) {
            grid.map.un('singleclick', handleMapClick);
            grid.map.removeLayer(grid.layer);
        }

        grid.map = map;
        grid.baseSize = baseSizeFor(map);
        grid.selectedCell = null;
        grid.hoverCell = null;
        grid.layer = new ol.layer.Layer({ render: render, zIndex: 996 });
        map.addLayer(grid.layer);
        map.on('singleclick', handleMapClick);
    };

    window.uspsim2d5.enableGridSnapping = function (drawInteraction, olGeomType) {
        if (!grid.map) return;
        window.uspsim2d5.disableGridSnapping();

        const snapToCellCenter = olGeomType === 'Point';

        grid.snapInteraction = new ol.interaction.Interaction({
            handleEvent: function (evt) {
                if (!evt.coordinate) return true;

                const spacing = currentSpacing();
                if (snapToCellCenter) {
                    const cell = cellAt(evt.coordinate, spacing);
                    if (evt.type === 'pointermove' && !sameCell(cell, grid.hoverCell)) {
                        grid.hoverCell = cell;
                        redraw();
                    }
                    evt.coordinate = cellCenter(cell);
                } else {
                    evt.coordinate = nearestDot(evt.coordinate, spacing);
                }
                return true;
            }
        });
        grid.map.addInteraction(grid.snapInteraction);

        if (snapToCellCenter && drawInteraction) {
            grid.snapDraw = drawInteraction;
            drawInteraction.on('drawend', selectPlacedCell);
        }
    };

    window.uspsim2d5.disableGridSnapping = function () {
        if (grid.snapInteraction && grid.map) grid.map.removeInteraction(grid.snapInteraction);
        if (grid.snapDraw) grid.snapDraw.un('drawend', selectPlacedCell);
        grid.snapInteraction = null;
        grid.snapDraw = null;
        grid.hoverCell = null;
        redraw();
    };
})();
