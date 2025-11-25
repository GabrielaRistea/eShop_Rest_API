document.addEventListener('DOMContentLoaded', () => {
    // extragere id din url
    const params = new URLSearchParams(window.location.search);
    const productId = params.get('id');

    if (!productId) {
        document.getElementById('main-container').innerHTML =
            '<div class="alert alert-warning">Produs invalid. <a href="produse.html">Înapoi la magazin</a></div>';
        return;
    }

    incarcaDetaliiProdus(productId);
});

async function incarcaDetaliiProdus(id) {
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

                    <hr class="my-4">

                    <div class="d-flex align-items-center mb-4">
                        <span class="me-3 fw-bold">Stoc: ${p.stock > 0 ? p.stock + ' buc.' : '<span class="text-danger">Epuizat</span>'}</span>
                    </div>

                    <div class="d-grid gap-2 d-md-block">
                        <button class="btn btn-primary btn-lg px-4" ${p.stock === 0 ? 'disabled' : ''}>
                            Adaugă în Coș 🛒
                        </button>
                        <a href="products.html" class="btn btn-outline-secondary btn-lg px-4">
                            Înapoi
                        </a>
                    </div>
                </div>
            </div>
        `;

    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger">Eroare: ${err.message} <br> <a href="produse.html">Înapoi la produse</a></div>`;
    }
}