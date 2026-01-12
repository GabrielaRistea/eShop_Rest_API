
const URL_WISHLIST = "https://localhost:7052/Wishlist";

document.addEventListener('DOMContentLoaded', () => {
    if (!isLoggedIn()) {
        console.log("Utilizatorul nu este logat. Redirecționare...");
        window.location.href = 'login.html';
        return;
    }

    console.log("Utilizator logat detectat. Încărcăm wishlist-ul...");
    loadWishlist();
});

async function loadWishlist() {
    const container = document.getElementById('wishlist-container');

    try {
        const response = await fetch(URL_WISHLIST, {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (response.ok) {
            const products = await response.json();
            renderProducts(products);
        } else {
            if (response.status === 401) {
                alert("Sesiunea a expirat. Te rugăm să te loghezi din nou.");
                window.location.href = 'login.html';
            } else {
                container.innerHTML = `<p class="text-center text-danger">Eroare la încărcare (Status: ${response.status})</p>`;
            }
        }
    } catch (error) {
        console.error("Eroare la fetch:", error);
        container.innerHTML = `<p class="text-center text-danger">Nu s-a putut contacta serverul. Verifică portul API-ului!</p>`;
    }
}

function renderProducts(products) {
    const container = document.getElementById('wishlist-container');
    container.innerHTML = '';

    if (!products || products.length === 0) {
        container.innerHTML = '<div class="col-12 text-center py-5"><p class="lead">Wishlist-ul tău este gol momentan. 🍰</p></div>';
        return;
    }

    products.forEach(p => {
        const card = `
            <div class="col-md-4 col-lg-3 mb-4" id="product-${p.id}">
                <div class="card h-100 shadow-sm border-0">
                 <a href="product-details.html?id=${p.id}" style="text-decoration: none; color: inherit;">
                    <img src="${p.productImage ? 'data:image/jpeg;base64,' + p.productImage : 'img/no-image.jpg'}" 
                         class="card-img-top" style="height: 200px; object-fit: cover;" alt="${p.name}">
                         </a>
                    <div class="card-body text-center">
                        <h5 class="card-title fw-bold">${p.name}</h5>
                        <p class="text-danger fw-bold fs-5">${p.price} RON</p>
                        <button class="btn btn-sm btn-outline-danger w-100" onclick="removeFromWishlist(${p.id})">
                            🗑️ Elimină
                        </button>
                    </div>
                </div>
            </div>`;
        container.innerHTML += card;
    });
}

async function removeFromWishlist(productId) {
    if (!confirm("Sigur elimini acest produs?")) return;

    try {
        const response = await fetch(`${URL_WISHLIST}/remove/${productId}`, {
            method: 'DELETE',
            headers: getAuthHeaders() 
        });

        if (response.ok) {
            document.getElementById(`product-${productId}`).remove();
        } else {
            alert("Eroare la ștergere.");
        }
    } catch (err) {
        console.error(err);
    }
}