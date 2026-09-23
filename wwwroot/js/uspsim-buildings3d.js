window.uspsim2d5 = window.uspsim2d5 || {};

// Static Style Cache (Zero allocation overhead)
const STATIC_STYLES = {
    shadow: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(15, 23, 42, 0.12)' })
    }),
    wallLow: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(203, 213, 225, 0.60)' }),
        stroke: new ol.style.Stroke({ color: 'rgba(148, 163, 184, 0.50)', width: 0.8 })
    }),
    roofLow: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(226, 232, 240, 0.70)' }),
        stroke: new ol.style.Stroke({ color: 'rgba(148, 163, 184, 0.60)', width: 1.0 })
    }),
    wallStd: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(148, 163, 184, 0.85)' }),
        stroke: new ol.style.Stroke({ color: 'rgba(71, 85, 105, 0.45)', width: 0.8 })
    }),
    roofStd: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(238, 242, 252, 0.94)' }),
        stroke: new ol.style.Stroke({ color: 'rgba(100, 116, 139, 0.65)', width: 1.0 })
    }),
    wallHigh: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(100, 116, 139, 0.92)' }),
        stroke: new ol.style.Stroke({ color: 'rgba(51, 65, 85, 0.80)', width: 1.0 })
    }),
    roofHigh: new ol.style.Style({
        fill: new ol.style.Fill({ color: 'rgba(255, 255, 255, 0.98)' }),
        stroke: new ol.style.Stroke({ color: 'rgba(71, 85, 105, 0.85)', width: 1.2 })
    })
};

window.uspsim2d5.renderBuildings = function (geoJsonInput) {
    if (!geoJsonInput || !window.ol) {
        console.warn("[2.5D Log] Aborting: Missing GeoJSON data or OpenLayers window.ol is undefined.");
        return;
    }

    try {
        const startTime = performance.now();
        const geoJsonObj = typeof geoJsonInput === 'string' ? JSON.parse(geoJsonInput) : geoJsonInput;
        const format = new ol.format.GeoJSON();
        const rawFeatures = format.readFeatures(geoJsonObj, { featureProjection: 'EPSG:3857' });

        if (!rawFeatures || rawFeatures.length === 0) {
            console.warn("[2.5D Log] Aborting: 0 features found in GeoJSON.");
            return;
        }

        console.log(`[2.5D Log] Grouping ${rawFeatures.length} building footprints into consolidated features...`);
        window._buildingFeaturesStore = rawFeatures;
        const buildingMetadataList = [];

        rawFeatures.forEach(feature => {
            const geom = feature.getGeometry();
            if (!geom) return;

            const geomType = geom.getType();
            let polygonRingsList = [];
            if (geomType === 'Polygon') {
                polygonRingsList.push(geom.getCoordinates());
            } else if (geomType === 'MultiPolygon') {
                polygonRingsList = geom.getCoordinates();
            } else {
                return;
            }

            const props = feature.getProperties() || {};
            const hRoof = props.b3_h_70p ?? props.b3_h_max ?? props.b3_h_50p ?? 0;
            const hGround = props.b3_h_maaiveld ?? 0;

            let heightMeters = Math.max(hRoof - hGround, 1.5);
            if (props.b3_is_glas_dak || props.status === 'Pand buiten gebruik') {
                heightMeters = Math.min(heightMeters, 5.0);
            }

            const heightPixels = Math.min(Math.max(heightMeters * 0.48, 3), 32);

            buildingMetadataList.push({
                heightPixels: heightPixels,
                heightMeters: heightMeters,
                ringsList: polygonRingsList
            });
        });

        const shadowFeature = new ol.Feature();
        const wallLowFeature = new ol.Feature();
        const roofLowFeature = new ol.Feature();
        const wallStdFeature = new ol.Feature();
        const roofStdFeature = new ol.Feature();
        const wallHighFeature = new ol.Feature();
        const roofHighFeature = new ol.Feature();

        shadowFeature.setStyle(STATIC_STYLES.shadow);
        wallLowFeature.setStyle(STATIC_STYLES.wallLow);
        roofLowFeature.setStyle(STATIC_STYLES.roofLow);
        wallStdFeature.setStyle(STATIC_STYLES.wallStd);
        roofStdFeature.setStyle(STATIC_STYLES.roofStd);
        wallHighFeature.setStyle(STATIC_STYLES.wallHigh);
        roofHighFeature.setStyle(STATIC_STYLES.roofHigh);

        let lastResolution = -1;

        const updateExtrusionGeometries = function (resolution) {
            if (resolution === lastResolution) return;
            lastResolution = resolution;

            const shadowRingsList = [];
            const wallLowRingsList = [];
            const roofLowRingsList = [];
            const wallStdRingsList = [];
            const roofStdRingsList = [];
            const wallHighRingsList = [];
            const roofHighRingsList = [];

            buildingMetadataList.forEach(item => {
                const dx = item.heightPixels * resolution * 0.45;
                const dy = item.heightPixels * resolution * 0.75;

                item.ringsList.forEach(rings => {
                    if (!rings || rings.length === 0) return;
                    shadowRingsList.push(rings);

                    const targetWallList = item.heightMeters <= 4 ? wallLowRingsList : (item.heightMeters >= 35 ? wallHighRingsList : wallStdRingsList);
                    const targetRoofList = item.heightMeters <= 4 ? roofLowRingsList : (item.heightMeters >= 35 ? roofHighRingsList : roofStdRingsList);

                    rings.forEach(ring => {
                        if (!ring || ring.length < 3) return;

                        for (let i = 0; i < ring.length - 1; i++) {
                            const p1 = ring[i];
                            const p2 = ring[i + 1];

                            const wallPoly = [
                                p1,
                                p2,
                                [p2[0] + dx, p2[1] + dy],
                                [p1[0] + dx, p1[1] + dy],
                                p1
                            ];
                            targetWallList.push([wallPoly]);
                        }
                    });

                    const offsetRings = rings.map(ring => ring.map(pt => [pt[0] + dx, pt[1] + dy]));
                    targetRoofList.push(offsetRings);
                });
            });

            if (shadowRingsList.length > 0) shadowFeature.setGeometry(new ol.geom.MultiPolygon(shadowRingsList));
            if (wallLowRingsList.length > 0) wallLowFeature.setGeometry(new ol.geom.MultiPolygon(wallLowRingsList));
            if (roofLowRingsList.length > 0) roofLowFeature.setGeometry(new ol.geom.MultiPolygon(roofLowRingsList));
            if (wallStdRingsList.length > 0) wallStdFeature.setGeometry(new ol.geom.MultiPolygon(wallStdRingsList));
            if (roofStdRingsList.length > 0) roofStdFeature.setGeometry(new ol.geom.MultiPolygon(roofStdRingsList));
            if (wallHighRingsList.length > 0) wallHighFeature.setGeometry(new ol.geom.MultiPolygon(wallHighRingsList));
            if (roofHighRingsList.length > 0) roofHighFeature.setGeometry(new ol.geom.MultiPolygon(roofHighRingsList));
        };

        const consolidatedFeatures = [
            shadowFeature,
            wallLowFeature, roofLowFeature,
            wallStdFeature, roofStdFeature,
            wallHighFeature, roofHighFeature
        ];

        const initialZIndex = window._desiredZIndices["pdok-3dbag-buildings"] ?? 100;
        const initialVisible = window._desiredVisibilities["pdok-3dbag-buildings"] ?? true;

        const vectorSource = new ol.source.Vector({ features: consolidatedFeatures });
        const vectorLayer = new ol.layer.Vector({
            source: vectorSource,
            zIndex: initialZIndex,
            visible: initialVisible
        });

        window._mapLayers["pdok-3dbag-buildings"] = vectorLayer;

        const tryAddToMap = function () {
            let mapInstance = window.activeOlMap;

            if (mapInstance) {
                if (window._current2D5Layer) {
                    try { mapInstance.removeLayer(window._current2D5Layer); } catch (e) { }
                }

                mapInstance.getView().on('change:resolution', function () {
                    updateExtrusionGeometries(mapInstance.getView().getResolution());
                });

                updateExtrusionGeometries(mapInstance.getView().getResolution());

                mapInstance.addLayer(vectorLayer);
                window._current2D5Layer = vectorLayer;
                mapInstance.render();

                const elapsedMs = Math.round(performance.now() - startTime);
                console.log(`[2.5D Log] Rendered ${rawFeatures.length} buildings in ${elapsedMs}ms.`);
                return true;
            } else {
                console.warn("[2.5D Log] Map instance not bound yet, queueing building layer render...");
                window._pendingLayersToRender.push(tryAddToMap);
                return false;
            }
        };

        tryAddToMap();

    } catch (err) {
        console.error("[2.5D Log] Fatal error rendering building layer:", err);
    }
};
