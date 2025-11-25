
// definire url-urile api 
const API_BASE = window.location.origin; // ex: http://localhost:5157
const URL_CATEGORII = `${API_BASE}/Category`;
const URL_PRODUSE = `${API_BASE}/Product`;

// functie pentru erori
function afiseazaEroare(elementId, mesaj) {
    const el = document.getElementById(elementId);
    if (el) el.innerHTML = `<div class="alert alert-danger">${mesaj}</div>`;
}