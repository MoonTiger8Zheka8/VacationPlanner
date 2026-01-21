window.leafletMap = {
  map: null,

  markersById: {},   
  points: [],        
  polyline: null,
  polylineHighlight: null,

  init: function (mapId, lat, lon, zoom) {
    if (this.map) {
      this.map.remove();
      this.map = null;
    }

    this.markersById = {};
    this.points = [];
    this.polyline = null;
    this.polylineHighlight = null;

    this.map = L.map(mapId).setView([lat, lon], zoom);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap'
    }).addTo(this.map);
  },

  _createNumberIcon: function (n) {
    // HTML marker with number
    return L.divIcon({
      className: 'numbered-marker',
      html: `<div class="marker-pin">${n}</div>`,
      iconSize: [30, 30],
      iconAnchor: [15, 30],
      popupAnchor: [0, -28]
    });
  },

  addNumberedMarker: function (id, order, lat, lon, text) {
    if (!this.map) return;

    const icon = this._createNumberIcon(order);

    const marker = L.marker([lat, lon], { icon: icon }).addTo(this.map);
    marker.bindPopup(`<b>${order}. ${text}</b>`);

    this.markersById[id] = marker;
    this.points.push({ id, lat, lon, text, order });
  },

  drawRoute: function () {
    if (!this.map || this.points.length < 2) return;

    // сорт
    const sorted = [...this.points].sort((a, b) => a.order - b.order);
    const latlngs = sorted.map(p => [p.lat, p.lon]);

    if (this.polyline) this.map.removeLayer(this.polyline);
    if (this.polylineHighlight) this.map.removeLayer(this.polylineHighlight);

    // лінія маршруту
    this.polyline = L.polyline(latlngs, { weight: 4, opacity: 0.5 }).addTo(this.map);

    this.polylineHighlight = L.polyline(latlngs, { weight: 8, opacity: 0.0 }).addTo(this.map);
  },

  fitBounds: function () {
    if (!this.map) return;

    if (this.polyline) {
      this.map.fitBounds(this.polyline.getBounds().pad(0.3));
      return;
    }

    const ids = Object.keys(this.markersById);
    if (ids.length === 0) return;

    const markers = ids.map(id => this.markersById[id]);
    const group = new L.featureGroup(markers);
    this.map.fitBounds(group.getBounds().pad(0.3));
  },

  selectPlace: function (id) {
    if (!this.map) return;

    const marker = this.markersById[id];
    if (!marker) return;

    marker.openPopup();
    this.map.setView(marker.getLatLng(), 14);

    // підсвічування лінії
    if (this.polylineHighlight) {
      this.polylineHighlight.setStyle({ opacity: 0.9 });
    }
  },

  clearSelection: function () {
    if (this.polylineHighlight) {
      this.polylineHighlight.setStyle({ opacity: 0.0 });
    }
  }
};
