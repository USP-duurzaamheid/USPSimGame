window.uspsim2d5 = window.uspsim2d5 || {};

window.uspsim2d5.renderTeamAreas = function (teamsPayloadList) {
    const map = window.activeOlMap;
    if (!map) {
        window._pendingLayersToRender.push(function () {
            window.uspsim2d5.renderTeamAreas(teamsPayloadList);
        });
        return;
    }

    const geojsonFormat = new ol.format.GeoJSON();
    window._teamAreaGeometries = [];

    if (!window._teamAreasSource) {
        window._teamAreasSource = new ol.source.Vector();
        window._teamAreasLayer = new ol.layer.Vector({
            source: window._teamAreasSource,
            style: function (feature) {
                const color = feature.get('_teamColor') || '#3b82f6';
                const fillColor = feature.get('_fillColor') || 'rgba(59, 130, 246, 0.30)';
                return new ol.style.Style({
                    fill: new ol.style.Fill({ color: fillColor }),
                    stroke: new ol.style.Stroke({ color: color, width: 3, lineDash: [6, 4] })
                });
            },
            zIndex: 990
        });
        map.addLayer(window._teamAreasLayer);
    } else if (!map.getLayers().getArray().includes(window._teamAreasLayer)) {
        map.addLayer(window._teamAreasLayer);
    }

    window._teamAreasSource.clear();

    if (Array.isArray(teamsPayloadList)) {
        const self = this;
        teamsPayloadList.forEach(function (team) {
            if (team.areaDefinition) {
                try {
                    const feats = geojsonFormat.readFeatures(team.areaDefinition, {
                        dataProjection: 'EPSG:4326',
                        featureProjection: map.getView().getProjection()
                    });
                    const hexColor = team.color || '#3b82f6';
                    const fillColor = self.hexToRgba ? self.hexToRgba(hexColor, 0.30) : 'rgba(59, 130, 246, 0.30)';

                    feats.forEach(function (f) {
                        f.set('_teamName', team.name);
                        f.set('_teamColor', hexColor);
                        f.set('_fillColor', fillColor);

                        window._teamAreaGeometries.push({
                            geometry: f.getGeometry(),
                            color: hexColor,
                            teamName: team.name
                        });

                        window._teamAreasSource.addFeature(f);
                    });
                } catch (e) {
                    console.warn('[uspsim2d5] Error parsing team area GeoJSON:', e);
                }
            }
        });
    }
};

window.uspsim2d5.toggleTeamAreasVisibility = function (visible) {
    if (window._teamAreasLayer) {
        window._teamAreasLayer.setVisible(visible);
    }
};

window.uspsim2d5.refreshTeamAreas = function (sessionId) {
    const sid = sessionId || window._currentSessionId;
    if (sid) {
        fetch(`/api/teams/session/${sid}`)
            .then(r => r.json())
            .then(teams => {
                window.uspsim2d5.renderTeamAreas(teams);
            })
            .catch(e => console.warn('[uspsim2d5] Error fetching team areas:', e));
    }
};
