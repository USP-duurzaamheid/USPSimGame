window.uspsim2d5 = window.uspsim2d5 || {};

window.uspsim2d5.renderPlanFeatures = function (featuresPayloadList, fallbackColor) {
    this.clearPlanHighlight();
    const map = window.activeOlMap;
    if (!map || !featuresPayloadList) return;

    try {
        const geojsonFormat = new ol.format.GeoJSON();

        if (!window._highlightSource) {
            window._highlightSource = new ol.source.Vector();
            window._highlightLayer = new ol.layer.Vector({
                source: window._highlightSource,
                style: function (feature) {
                    const color = feature.get('_planColor') || fallbackColor || '#10b981';
                    const isPulsing = feature.get('_isPulsing');
                    const pulseScale = feature.get('_pulseScale') || 1.0;

                    if (isPulsing) {
                        return new ol.style.Style({
                            fill: new ol.style.Fill({ color: 'rgba(245, 158, 11, ' + (0.45 * pulseScale) + ')' }),
                            stroke: new ol.style.Stroke({
                                color: '#f59e0b',
                                width: Math.round(5 * pulseScale)
                            }),
                            image: new ol.style.Circle({
                                radius: Math.round(9 * pulseScale),
                                fill: new ol.style.Fill({ color: '#f59e0b' }),
                                stroke: new ol.style.Stroke({ color: '#ffffff', width: 3 })
                            })
                        });
                    }

                    return new ol.style.Style({
                        fill: new ol.style.Fill({ color: 'rgba(16, 185, 129, 0.30)' }),
                        stroke: new ol.style.Stroke({ color: color, width: 4 }),
                        image: new ol.style.Circle({
                            radius: 8,
                            fill: new ol.style.Fill({ color: color }),
                            stroke: new ol.style.Stroke({ color: '#ffffff', width: 2 })
                        })
                    });
                },
                zIndex: 995
            });
            map.addLayer(window._highlightLayer);
        } else if (!map.getLayers().getArray().includes(window._highlightLayer)) {
            map.addLayer(window._highlightLayer);
        }

        window._highlightSource.clear();

        const allFeatures = [];
        if (typeof featuresPayloadList === 'string') {
            const features = geojsonFormat.readFeatures(featuresPayloadList, {
                dataProjection: 'EPSG:4326',
                featureProjection: map.getView().getProjection()
            });
            features.forEach(function (f) { allFeatures.push(f); });
        } else if (Array.isArray(featuresPayloadList)) {
            featuresPayloadList.forEach(function (item) {
                if (item.geoJson) {
                    const feats = geojsonFormat.readFeatures(item.geoJson, {
                        dataProjection: 'EPSG:4326',
                        featureProjection: map.getView().getProjection()
                    });
                    feats.forEach(function (f) {
                        if (item.color) f.set('_planColor', item.color);
                        if (item.layerName) f.set('_layerName', item.layerName);
                        if (item.category) f.set('_category', item.category);
                        if (item.propertiesJson) f.set('_customPropertiesJson', item.propertiesJson);
                        allFeatures.push(f);
                    });
                }
            });
        }

        allFeatures.forEach(function (f) {
            f.set('_isPulsing', true);
            f.set('_pulseScale', 1.0);
        });
        window._highlightSource.addFeatures(allFeatures);

        // 2-second glowing pulse animation (2000ms)
        if (window._pulseTimer) {
            clearInterval(window._pulseTimer);
        }
        let elapsed = 0;
        const duration = 2000;
        const interval = 50;

        window._pulseTimer = setInterval(function () {
            elapsed += interval;
            const progress = elapsed / duration;
            if (progress < 1.0) {
                const scale = 1.0 + 0.35 * Math.sin(progress * Math.PI * 4);
                allFeatures.forEach(function (f) {
                    f.set('_pulseScale', scale);
                });
                if (window._highlightLayer) {
                    window._highlightLayer.changed();
                }
            } else {
                clearInterval(window._pulseTimer);
                window._pulseTimer = null;
                allFeatures.forEach(function (f) {
                    f.set('_isPulsing', false);
                    f.set('_pulseScale', 1.0);
                });
                if (window._highlightLayer) {
                    window._highlightLayer.changed();
                }
            }
        }, interval);

        map.render();
    } catch (err) {
        console.error('[uspsim2d5] Error rendering plan features:', err);
    }
};

window.uspsim2d5.clearPlanHighlight = function () {
    if (window._pulseTimer) {
        clearInterval(window._pulseTimer);
        window._pulseTimer = null;
    }
    if (window._highlightSource) {
        window._highlightSource.clear();
    }
};

window.uspsim2d5.enableMapFeatureInspection = function (dotNetRef) {
    window._blazorInspectDotNetRef = dotNetRef;
    const map = window.activeOlMap;
    if (!map) return;

    if (window._mapFeatureInspectClickListener) {
        map.un('singleclick', window._mapFeatureInspectClickListener);
    }

    window._mapFeatureInspectClickListener = function (evt) {
        if (window._drawInteraction && window._drawInteraction.getActive()) {
            return;
        }

        const pixel = evt.pixel;
        const clickedFeatures = [];

        map.forEachFeatureAtPixel(pixel, function (feature, layer) {
            if (!feature) return;
            if (layer === window._featureHighlightLayer) return;
            if (window._mapLayers && layer === window._mapLayers['pdok-3dbag-buildings']) return;

            let layerName = 'Map Feature';
            let layerKey = 'unknown';
            let category = 'Base Map';
            let color = '#3b82f6';
            let isEditable = false;
            let layerId = null;

            if (window._draftVectorLayers) {
                Object.keys(window._draftVectorLayers).forEach(id => {
                    if (window._draftVectorLayers[id].layer === layer) {
                        isEditable = true;
                        layerId = id;
                        layerName = feature.get('_layerName') || `Plannable Layer (${id})`;
                        color = window._draftVectorLayers[id].strokeColor || '#3b82f6';
                        category = 'Draft Plan Geometry';
                    }
                });
            }

            if (!isEditable) {
                if (layer === window._highlightLayer) {
                    layerName = feature.get('_layerName') || 'Spatial Plan Geometry';
                    category = feature.get('_category') || 'Spatial Plan';
                    color = feature.get('_planColor') || '#10b981';
                    layerId = 'active_plan_feature';
                } else if (layer === window._teamAreasLayer) {
                    layerName = feature.get('_teamName') || 'Existing Team Area';
                    category = 'Territory';
                    color = feature.get('_teamColor') || '#3b82f6';
                    layerId = 'team_area';
                } else if (window._mapLayers) {
                    Object.keys(window._mapLayers).forEach(k => {
                        if (window._mapLayers[k] === layer) {
                            layerKey = k;
                            layerName = k;
                            category = 'Base Layer';
                            layerId = k;
                        }
                    });
                }
            }

            const rawProps = feature.getProperties();
            const cleanProps = {};
            Object.keys(rawProps).forEach(key => {
                if (key !== 'geometry' && !key.startsWith('_')) {
                    const val = rawProps[key];
                    if (val !== null && val !== undefined && typeof val !== 'object') {
                        cleanProps[key] = String(val);
                    }
                }
            });

            if (rawProps._customPropertiesJson) {
                try {
                    const customObj = JSON.parse(rawProps._customPropertiesJson);
                    Object.assign(cleanProps, customObj);
                } catch (e) { }
            }

            clickedFeatures.push({
                featureId: String(feature.getId() || Math.random()),
                layerKey: layerKey,
                layerId: layerId || layerKey,
                layerName: layerName,
                category: category,
                color: color,
                isEditable: isEditable,
                properties: cleanProps,
                customEntries: []
            });
        }, { hitTolerance: 8 });

        if (window._buildingFeaturesStore && window._buildingFeaturesStore.length > 0) {
            const buildingLayer = window._mapLayers ? window._mapLayers['pdok-3dbag-buildings'] : null;
            if (!buildingLayer || buildingLayer.getVisible()) {
                const coord = evt.coordinate;
                for (let i = 0; i < window._buildingFeaturesStore.length; i++) {
                    const bFeat = window._buildingFeaturesStore[i];
                    const geom = bFeat.getGeometry();
                    if (geom && geom.intersectsCoordinate(coord)) {
                        const rawProps = bFeat.getProperties() || {};
                        const cleanProps = {};
                        Object.keys(rawProps).forEach(key => {
                            if (key !== 'geometry' && !key.startsWith('_')) {
                                const val = rawProps[key];
                                if (val !== null && val !== undefined && typeof val !== 'object') {
                                    cleanProps[key] = String(val);
                                }
                            }
                        });

                        const bId = rawProps.identificatie || rawProps.b3_bag_id || rawProps.gml_id || `Building_${i}`;
                        clickedFeatures.push({
                            featureId: String(bId),
                            layerKey: 'pdok-3dbag-buildings',
                            layerId: 'pdok-3dbag-buildings',
                            layerName: '3D BAG Building',
                            category: 'BAG Building (PDOK)',
                            color: '#64748b',
                            isEditable: false,
                            properties: cleanProps,
                            customEntries: []
                        });
                        break;
                    }
                }
            }
        }

        if (clickedFeatures.length > 0 && window._blazorInspectDotNetRef) {
            const clientX = evt.originalEvent ? evt.originalEvent.clientX : pixel[0];
            const clientY = evt.originalEvent ? evt.originalEvent.clientY : pixel[1];
            window._blazorInspectDotNetRef.invokeMethodAsync('OnMapFeaturesInspected', clickedFeatures, clientX, clientY);
        }
    };

    map.on('singleclick', window._mapFeatureInspectClickListener);
};

window.uspsim2d5.updateDraftFeatureProperties = function (layerId, propertiesJson) {
    if (window._draftVectorLayers && window._draftVectorLayers[layerId]) {
        const source = window._draftVectorLayers[layerId].source;
        const feats = source.getFeatures();
        if (feats && feats.length > 0) {
            feats.forEach(f => f.set('_customPropertiesJson', propertiesJson));
        }
    }
};

window.uspsim2d5.clearFeatureHighlight = function () {
    if (window._featureHighlightSource) {
        window._featureHighlightSource.clear();
    }
};
