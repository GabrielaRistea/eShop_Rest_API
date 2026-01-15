
// definire url-urile api 
const API_BASE = window.location.origin; // ex: http://localhost:5157
const URL_CATEGORII = `${API_BASE}/Category`;
const URL_PRODUSE = `${API_BASE}/Product`;


// functie pentru erori
function afiseazaEroare(elementId, mesaj) {
    const el = document.getElementById(elementId);
    if (el) el.innerHTML = `<div class="alert alert-danger">${mesaj}</div>`;
}

const URL_REGISTER = `${API_BASE}/api/Account/register`;
const URL_LOGIN = `${API_BASE}/login`;

const ACCESS_TOKEN_KEY = 'accessToken';

// cheia pentru roluri
const USER_ROLES_KEY = 'userRoles';

function getToken() {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
}

function isLoggedIn() {
    return !!getToken();
}

function logout() {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(USER_ROLES_KEY); 
    window.location.href = 'index.html';
}

// obtine headerele necesare (inclusiv auhtorization)
function getAuthHeaders() {
    const headers = {
        'Content-Type': 'application/json'
    };
    const token = getToken();
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
}

// actualizare navbar
function updateAuthUI() {
    const authContainer = document.getElementById('auth-links');
    if (!authContainer) return;

    if (isLoggedIn()) {
        authContainer.innerHTML = `
            <li class="nav-item">
                <a class="nav-link" href="#" onclick="logout()">Deconectare</a>
            </li>
        `;
    } else {
        authContainer.innerHTML = `
            <li class="nav-item"><a class="nav-link" href="login.html">Autentificare</a></li>
            <li class="nav-item"><a class="nav-link" href="register.html">Înregistrare</a></li>
        `;
    }

    const adminLink = document.querySelector('a[href="admin.html"]');
    if (adminLink) {
        if (isAdmin()) {
            adminLink.style.display = 'block';
        } else {
            adminLink.style.display = 'none';
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    updateAuthUI();
});

function isAdmin() {
    const rolesJson = localStorage.getItem(USER_ROLES_KEY);
    if (!rolesJson) return false;

    try {
        const roles = JSON.parse(rolesJson);

        return Array.isArray(roles) && roles.includes('Admin');
    } catch (e) {
        return false;
    }
}

async function addToCart(productId, quantity = 1) {
    if (!isLoggedIn()) {
        alert("Trebuie să fii logată pentru a adauga produse in cos!");
        window.location.href = 'login.html';
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/Orders/add`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({
                productId: parseInt(productId), 
                quantity: parseInt(quantity)    
            })
        });

        if (response.ok) {
            alert("✅ Produs adăugat în coș!");
        } else {
            if (response.status === 401) {
                alert("Sesiunea a expirat. Te rugăm să te loghezi din nou.");
                logout();
                return;
            }

            const errorData = await response.json();
            alert(`❌ Eroare: ${errorData.message || 'Nu s-a putut adăuga produsul.'}`);
        }
    } catch (error) {
        console.error("Eroare rețea:", error);
        alert(`Eroare de conexiune la server: ${API_BASE}`);
    }
}