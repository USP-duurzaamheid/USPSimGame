(function () {
    // Intercept DotNet / DotNetObjectReference to absorb OpenLayers.Blazor internal shape click lookup fallbacks
    const patchDotNet = function () {
        if (window.DotNet && window.DotNet.invokeMethodAsync && !window._dotNetInvokePatched) {
            window._dotNetInvokePatched = true;
            const origInvoke = window.DotNet.invokeMethodAsync;
            window.DotNet.invokeMethodAsync = function (assembly, method, ...args) {
                const p = origInvoke.apply(this, arguments);
                if (method === 'OnInternalShapeClick') {
                    return p.catch(function (err) {
                        return null;
                    });
                }
                return p;
            };
        }
    };
    patchDotNet();
    setTimeout(patchDotNet, 100);
    setTimeout(patchDotNet, 500);

    // Intercept ol.Map constructor to capture map instance automatically
    if (window.ol && window.ol.Map && !window.olMapIntercepted) {
        window.olMapIntercepted = true;
        const OriginalMap = window.ol.Map;
        window.ol.Map = function (options) {
            const map = new OriginalMap(options);
            window.activeOlMap = map;
            console.log("[2.5D Log] Intercepted ol.Map instance successfully! Target element ID:", options.target);
            if (window._pendingLayersToRender && window._pendingLayersToRender.length > 0) {
                const pending = window._pendingLayersToRender.slice();
                window._pendingLayersToRender = [];
                pending.forEach(fn => fn());
            }
            return map;
        };
        window.ol.Map.prototype = OriginalMap.prototype;
    }
})();

window._mapLayers = window._mapLayers || {};
window._desiredZIndices = window._desiredZIndices || {};
window._desiredVisibilities = window._desiredVisibilities || {};
window._pendingLayersToRender = window._pendingLayersToRender || [];
window._draftVectorLayers = window._draftVectorLayers || {};
window._currentDraftLayerId = null;

window.uspsim2d5 = window.uspsim2d5 || {};

window.uspsim2d5.configureMap = function (mapOL) {
    console.log("[2.5D Log] ConfigureJsMethod invoked with mapOL object:", mapOL);
    if (mapOL) {
        window.activeOlMap = mapOL.Map ? mapOL.Map : mapOL;
        console.log("[2.5D Log] Captured OpenLayers map instance successfully:", window.activeOlMap);

        if (window._pendingLayersToRender && window._pendingLayersToRender.length > 0) {
            const pending = window._pendingLayersToRender.slice();
            window._pendingLayersToRender = [];
            pending.forEach(fn => fn());
        }
    }
};

window.uspsim2d5.loadSessionLayer = function (sessionId, layerKey) {
    console.log(`[Map Control] Fetching layer '${layerKey}' for Session #${sessionId} via HTTP API...`);
    const apiUrl = `/api/layers/${sessionId}/${encodeURIComponent(layerKey)}`;

    fetch(apiUrl)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status} when fetching layer '${layerKey}'`);
            }
            return response.json();
        })
        .then(geoJsonObj => {
            console.log(`[Map Control] Successfully fetched GeoJSON payload for '${layerKey}' via HTTP API! Features count: ${geoJsonObj.features ? geoJsonObj.features.length : 0}`);
            if (layerKey === 'pdok-3dbag-buildings') {
                window.uspsim2d5.renderBuildings(geoJsonObj);
            } else {
                window.uspsim2d5.renderInfrastructureLayer(geoJsonObj, layerKey);
            }
        })
        .catch(err => {
            console.error(`[Map Control] Error loading layer '${layerKey}':`, err);
        });
};

window.uspsim2d5.setLayerVisibility = function (layerKey, isVisible) {
    window._desiredVisibilities[layerKey] = isVisible;

    const layer = window._mapLayers ? window._mapLayers[layerKey] : null;
    if (layer) {
        layer.setVisible(isVisible);
        if (window.activeOlMap) window.activeOlMap.render();
    } else {
        console.log(`[Map Control] Stored desired visibility '${isVisible}' for pending layer '${layerKey}'.`);
    }
};

window.uspsim2d5.setLayerZIndex = function (layerKey, zIndex) {
    window._desiredZIndices[layerKey] = zIndex;

    const layer = window._mapLayers ? window._mapLayers[layerKey] : null;
    if (layer) {
        layer.setZIndex(zIndex);
        if (window.activeOlMap) window.activeOlMap.render();
    } else {
        console.log(`[Map Control] Stored desired zIndex '${zIndex}' for pending layer '${layerKey}'.`);
    }
};

window.uspsim2d5.hexToRgba = function (hex, alpha) {
    try {
        hex = (hex || '#3b82f6').trim().replace('#', '');
        if (hex.length === 6) {
            const r = parseInt(hex.substring(0, 2), 16);
            const g = parseInt(hex.substring(2, 4), 16);
            const b = parseInt(hex.substring(4, 6), 16);
            return `rgba(${r}, ${g}, ${b}, ${alpha})`;
        }
    } catch (e) { }
    return `rgba(59, 130, 246, ${alpha})`;
};

window.uspsim2d5.makeElementDraggable = function (elId) {
    const el = document.getElementById(elId);
    if (!el) return;

    const header = el.querySelector('.card-header') || el;
    let pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;

    header.style.cursor = 'move';
    header.onmousedown = dragMouseDown;

    function dragMouseDown(e) {
        e = e || window.event;
        if (e.target.tagName === 'BUTTON' || e.target.tagName === 'I' || e.target.classList.contains('btn-close')) return;
        e.preventDefault();
        pos3 = e.clientX;
        pos4 = e.clientY;
        document.onmouseup = closeDragElement;
        document.onmousemove = elementDrag;
    }

    function elementDrag(e) {
        e = e || window.event;
        e.preventDefault();
        pos1 = pos3 - e.clientX;
        pos2 = pos4 - e.clientY;
        pos3 = e.clientX;
        pos4 = e.clientY;

        const newTop = Math.max(10, Math.min(window.innerHeight - 80, el.offsetTop - pos2));
        const newLeft = Math.max(10, Math.min(window.innerWidth - 100, el.offsetLeft - pos1));

        el.style.top = newTop + "px";
        el.style.left = newLeft + "px";
    }

    function closeDragElement() {
        document.onmouseup = null;
        document.onmousemove = null;
    }
};

window.uspsim2d5.toggleImplementedFeaturesVisibility = function (isVisible) {
    const layer = window._mapLayers ? window._mapLayers['implemented-features-layer'] : null;
    if (layer) {
        layer.setVisible(isVisible);
        if (window.activeOlMap) window.activeOlMap.render();
    }
};
