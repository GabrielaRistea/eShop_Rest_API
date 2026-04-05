
document.addEventListener('DOMContentLoaded', () => {
    // extragere id din url
    const params = new URLSearchParams(window.location.search);
    const productId = params.get('id');

    if (!productId) {
        document.getElementById('main-container').innerHTML =
            '<div class="alert alert-warning">Produs invalid. <a href="produse.html">Înapoi la magazin</a></div>';
        return;
    }

    getProductDetails(productId);
});

async function getProductDetails(id) {
    const container = document.getElementById('main-container');

    try {
        // GET /Product/{id}
        const response = await fetch(`${URL_PRODUSE}/${id}`);

        if (!response.ok) {
            throw new Error('Produsul nu a fost găsit.');
        }

        const p = await response.json();

        let imageSrc = 'https://dummyimage.com/600x400/dee2e6/6c757d.jpg&text=Fara+Imagine';
        if (p.productImage) {
            imageSrc = `data:image/jpeg;base64,${p.productImage}`;
        }

        let pdfButtonHtml = '';
        if (p.pdfPath) {
            pdfButtonHtml = `
        <div class="mt-3">
            <a href="${p.pdfPath}" target="_blank" class="btn btn-outline-secondary">
                📄 Vezi Specificații PDF (Detalii complete)
            </a>
        </div>
    `;
        }

        container.innerHTML = `
            <div class="row bg-white shadow-sm rounded p-4">
                <div class="col-md-6 mb-4 mb-md-0">
                    <img src="${imageSrc}" class="img-fluid rounded w-100" alt="${p.name}" style="max-height: 500px; object-fit: contain;">
                </div>

                <div class="col-md-6">
                    <nav aria-label="breadcrumb">
                        <ol class="breadcrumb">
                            <li class="breadcrumb-item"><a href="products.html">Produse</a></li>
                            <li class="breadcrumb-item active" aria-current="page">${p.name}</li>
                        </ol>
                    </nav>

                    <h1 class="display-5 fw-bold mb-3">${p.name}</h1>
                    <div class="badge bg-secondary mb-3">${p.categoryName || 'Categorie Necunoscută'}</div>
                    
                    <h3 class="text-primary mb-4">${p.price} RON</h3>

                    <p class="lead text-muted mb-4">${p.description}</p>
                    ${pdfButtonHtml}
                    <hr class="my-4">

                    <div class="d-flex align-items-center mb-4">
                        <span class="me-3 fw-bold"> ${p.stock > 0 ? '<span class="text-danger">In Stoc</span>' : '<span class="text-danger">Stoc Epuizat</span>'}</span>
                    </div>

                    <div class="d-flex gap-3">
                        <button class="btn btn-primary btn-lg px-4" ${p.stock === 0 ? 'disabled' : ''} onclick="addToCart(${p.id})">
                           🛒 Adaugă în Coș 
                        </button>
                        <button class="btn btn-outline-danger btn-sm" onclick="addToWishlist(${p.id})" title="Adaugă la favorite">
                            ❤️  Favorite
                            </button>
                    </div>
                        <div class="mt-4">
                         <a href="products.html" class="text-muted text-decoration-none small">← Înapoi la listă</a>
                    </div>
                    </div>
                </div>
            </div>
        `;

    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger">Eroare: ${err.message} <br> <a href="produse.html">Înapoi la produse</a></div>`;
    }

    loadSimilarProducts(id);
}

async function loadSimilarProducts(productId) {
    const container = document.getElementById('main-container');

    try {
        const response = await fetch(`${URL_PRODUSE}/${productId}/similar`);
        if (!response.ok) return;

        const similarProducts = await response.json();

        if (similarProducts.length === 0) return; 

        let cardsHtml = '';
        similarProducts.forEach(p => {
            let thumb = p.productImage ? `data:image/jpeg;base64,${p.productImage}` : 'https://dummyimage.com/300x200/dee2e6/6c757d.jpg';

            cardsHtml += `
                <div class="col-md-3 col-sm-6 mb-4">
                    <div class="card h-100 shadow-sm">
                        <a href="product-details.html?id=${p.id}">
                            <img src="${thumb}" class="card-img-top" style="height: 150px; object-fit: cover;" alt="${p.name}">
                        </a>
                        <div class="card-body d-flex flex-column">
                            <h6 class="card-title fw-bold">${p.name}</h6>
                            <p class="text-primary mb-2">${p.price} RON</p>
                            <a href="product-details.html?id=${p.id}" class="btn btn-sm btn-outline-secondary mt-auto">Vezi Detalii</a>
                        </div>
                    </div>
                </div>
            `;
        });

        const similarSection = document.createElement('div');
        similarSection.className = 'mt-5';
        similarSection.innerHTML = `
            <h3 class="fw-bold mb-4 border-bottom pb-2">Produse Similare</h3>
            <div class="row">
                ${cardsHtml}
            </div>
        `;

        container.appendChild(similarSection);

    } catch (err) {
        console.error("Eroare la încărcarea produselor similare:", err);
    }
}

const URL_WISHLIST_ADD = "https://localhost:7052/Wishlist/add";

async function addToWishlist(productId) {
    if (!isLoggedIn()) {
        alert("Trebuie să fii logată pentru a salva produse în Wishlist!");
        window.location.href = 'login.html';
        return;
    }

    try {
        const response = await fetch(`${URL_WISHLIST_ADD}/${productId}`, {
            method: 'POST',
            headers: getAuthHeaders()
        });

        if (response.ok) {
            const data = await response.json();
            alert("✅ " + data.message);
        } else if (response.status === 400) {
            alert("ℹ️ Acest produs se află deja în Wishlist-ul tău.");
        } else if (response.status === 401) {
            alert("Sesiunea a expirat. Te rugăm să te loghezi din nou.");
            window.location.href = 'login.html';
        } else {
            alert("Eroare la adăugare. Status: " + response.status);
        }
    } catch (err) {
        console.error("Eroare server:", err);
        alert("Serverul nu a putut fi contactat.");
    }
}