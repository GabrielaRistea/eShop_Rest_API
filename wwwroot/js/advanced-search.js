document.addEventListener('DOMContentLoaded', () => {
    fetchAdvancedResults();
});

async function fetchAdvancedResults() {
    const urlParams = new URLSearchParams(window.location.search);
    const query = urlParams.get('q');
    const sort = document.getElementById('sort-order').value;
    const container = document.getElementById('lucene-results');

    if (!query) return;

    document.getElementById('search-title').textContent = `Rezultate pentru: "${query}"`;
    container.innerHTML = '<div class="text-center w-100 p-5"><div class="spinner-border text-primary"></div></div>';

    try {
        const response = await fetch(`/Product/advanced-search?q=${encodeURIComponent(query)}&sort=${sort}`);
        const results = await response.json();

        container.innerHTML = '';

        if (results.length === 0) {
            container.innerHTML = '<div class="alert alert-warning w-100 text-center">Nu s-a găsit nimic în fișele tehnice pentru acest termen.</div>';
            return;
        }

        results.forEach(res => {
            const imageSrc = `data:image/jpeg;base64,${res.productImage}`;

            const html = `
                <div class="col-md-4 mb-4">
                    <div class="card h-100 shadow-sm border-0 rounded-3 product-card">
                        <a href="product-details.html?id=${res.productID}">
                            <img src="${imageSrc}" class="card-img-top p-2" alt="${res.name}" 
                                 style="height: 200px; object-fit: cover; border-radius: 15px;">
                        </a>
                
                        <div class="card-body d-flex flex-column text-center">
                            <div class="mb-2">
                                <span class="badge bg-light text-success border border-success">
                                    Scor Lucene: ${res.luceneScore.toFixed(4)}
                                </span>
                            </div>

                            <h5 class="card-title fw-bold">
                                <a href="product-details.html?id=${res.productID}" style="text-decoration: none; color: inherit;" class="stretched-link-exception">
                                    ${res.name}
                                </a>
                            </h5>
                    
                            <div class="mt-auto d-flex justify-content-between align-items-center">
                                <span class="fs-5 fw-bold text-dark">${res.price} RON</span>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            container.insertAdjacentHTML('beforeend', html);
        });
    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger">Eroare: ${err.message}</div>`;
    }
}