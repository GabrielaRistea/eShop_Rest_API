// variabila globala pentru a memora toate produsele (cache pt search)
let allProducts = [];
let inputMin = document.getElementById('input-min');
let inputMax = document.getElementById('input-max');
let priceSlider = document.getElementById('price-slider');

document.addEventListener('DOMContentLoaded', () => {
    loadCategories();
    preloadProducts();
});

document.addEventListener('DOMContentLoaded', function () {

    // Configurare Slider
    noUiSlider.create(priceSlider, {
        start: [0, 200], // valori initiale
        connect: true,    
        step: 1,          // pas de 1 RON
        range: {
            'min': 0,
            'max': 200   // maximul absolut posibil
        },

        format: {
            to: value => Math.round(value),
            from: value => Number(value)
        }
    });

    // actualizare valori din casute atunci cand tragem de slider
    priceSlider.noUiSlider.on('update', function (values, handle) {
        if (handle === 0) {
            inputMin.value = values[0];
        } else {
            inputMax.value = values[1];
        }
    });

    // actualizare slider atunci cand scriem in casute
    inputMin.addEventListener('change', function () {
        priceSlider.noUiSlider.set([this.value, null]);
    });

    inputMax.addEventListener('change', function () {
        priceSlider.noUiSlider.set([null, this.value]);
    });
});

async function preloadProducts() {
    const container = document.getElementById('container-produse');
    container.innerHTML = '<div class="text-center w-100 mt-5"><div class="spinner-border text-primary"></div></div>';

    try {
        const response = await fetch(URL_PRODUSE);
        if (!response.ok) throw new Error("Eroare la încărcare produse");

        allProducts = await response.json();

        // afisare toate produsele
        renderProduse(allProducts);
    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger w-100">Eroare: ${err.message}</div>`;
    }
}

// incarcare categorii
async function loadCategories() {
    try {
        const response = await fetch(URL_CATEGORII);
        if (!response.ok) throw new Error("Eroare la categorii");
        const categorii = await response.json();

        const lista = document.getElementById('lista-categorii');

        categorii.forEach(cat => {
            const btn = document.createElement('button');
            btn.className = 'list-group-item list-group-item-action category-btn';
            btn.textContent = cat.name;
            btn.onclick = () => filterByCategory(cat.categoryID, cat.name, btn);
            lista.appendChild(btn);
        });
    } catch (err) {
        console.error(err);
    }
}

// incarcare produse filtrare dupa categorie
async function loadProductsByCategory(categoryId = null) {
    const container = document.getElementById('container-produse');
    container.innerHTML = '<div class="text-center w-100 mt-5"><div class="spinner-border text-primary"></div></div>';

    try {
        let url = categoryId
            ? `${URL_PRODUSE}/by-category-id/${categoryId}`
            : URL_PRODUSE;

        const response = await fetch(url);
        if (!response.ok) throw new Error("Eroare la produse");
        const produse = await response.json();

        renderProduse(produse);
    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger w-100">Nu am putut încărca produsele. (${err.message})</div>`;
    }
}

function renderProduse(produse) {
    const container = document.getElementById('container-produse');
    container.innerHTML = '';

    if (!produse || produse.length === 0) {
        container.innerHTML = '<div class="alert alert-info w-100">Nu au fost găsite produse.</div>';
        return;
    }

    produse.forEach(p => {
        let imageSrc = 'https://dummyimage.com/300x200/dee2e6/6c757d.jpg&text=Fara+Imagine';
        if (p.productImage) {
            imageSrc = `data:image/jpeg;base64,${p.productImage}`;
        }



        const html = `
    <div class="col">
        <div class="card h-100 shadow-sm product-card-hover">
            <a href="product-details.html?id=${p.id}" style="text-decoration: none; color: inherit;">
                <img src="${imageSrc}" class="card-img-top product-img" alt="${p.name}">
            </a>
            
            <div class="card-body d-flex flex-column">
                <h5 class="card-title">
                    <a href="product-details.html?id=${p.id}" style="text-decoration: none; color: inherit;" class="stretched-link-exception">
                        ${p.name}
                    </a>
                </h5>
                <p class="card-text text-truncate text-muted">${p.description}</p>
                <div class="mt-auto">
                    <div class="d-flex justify-content-between align-items-center">
                        <span class="fs-5 fw-bold text-dark">${p.price} RON</span>
                        <button class="btn btn-outline-primary btn-sm z-index-2">Adaugă</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
`;
        container.insertAdjacentHTML('beforeend', html);
    });
}


function filterByCategory(id, nume, btnElement) {
    document.getElementById('titlu-categorie').textContent = `Produse: ${nume}`;
    document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
    btnElement.classList.add('active');

    document.getElementById('search-input').value = '';
    const dropdown = document.getElementById('search-results-dropdown');
    if (dropdown) dropdown.style.display = 'none';

    loadProductsByCategory(id);
}

function resetFiltru(btnElement) {
    document.getElementById('titlu-categorie').textContent = 'Toate Produsele';
    document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
    btnElement.classList.add('active');

    document.getElementById('search-input').value = '';
    renderProduse(allProducts); 
}

// bara cautare
async function searchProducts() {
    const input = document.getElementById('search-input');
    const term = input.value.trim();
    const container = document.getElementById('container-produse');

    if (!term) {
        resetFiltru(document.querySelector('.category-btn'));
        return;
    }

    container.innerHTML = '<div class="text-center w-100 mt-5"><div class="spinner-border text-primary"></div></div>';
    document.getElementById('titlu-categorie').textContent = `Rezultate căutare: "${term}"`;

    try {
        const url = `${URL_PRODUSE}/by-product-name/${encodeURIComponent(term)}`;
        const response = await fetch(url);

        if (response.status === 404) {
            container.innerHTML = '<div class="alert alert-warning w-100">Nu am găsit produse cu acest nume.</div>';
            return;
        }

        if (!response.ok) throw new Error("Eroare la căutare");

        const produse = await response.json();
        renderProduse(produse);

    } catch (err) {
        container.innerHTML = `<div class="alert alert-danger w-100">Eroare: ${err.message}</div>`;
    }
}

// enter key listener
document.getElementById('search-input').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
        searchProducts();

        const dropdown = document.getElementById('search-results-dropdown');
        if (dropdown) dropdown.style.display = 'none';
    }
});


const searchInput = document.getElementById('search-input');
const dropdown = document.getElementById('search-results-dropdown');

// tastare input
searchInput.addEventListener('input', function (e) {
    const term = e.target.value.toLowerCase().trim();

    if (term.length === 0) {
        if (dropdown) dropdown.style.display = 'none';
        renderProduse(allProducts);
        document.getElementById('titlu-categorie').textContent = 'Toate Produsele';
        return;
    }

    const rezultate = allProducts.filter(p =>
        p.name.toLowerCase().includes(term)
    );

    document.getElementById('titlu-categorie').textContent = `Rezultate pentru: "${e.target.value}"`;

    renderProduse(rezultate);

    if (dropdown) renderDropdown(rezultate);
});

// lista dropdown search
function renderDropdown(produse) {
    dropdown.innerHTML = '';

    if (produse.length === 0) {
        dropdown.innerHTML = '<div class="p-3 text-muted text-center">Niciun rezultat.</div>';
        dropdown.style.display = 'block';
        return;
    }

    // afisare maxim 5 sugestii
    produse.slice(0, 5).forEach(p => {
        let thumb = 'https://dummyimage.com/40x40/dee2e6/6c757d.jpg';
        if (p.productImage) {
            thumb = `data:image/jpeg;base64,${p.productImage}`;
        }

        const div = document.createElement('div');
        div.className = 'search-result-item'; 
        div.innerHTML = `
            <img src="${thumb}" class="search-thumb">
            <div>
                <div class="fw-bold">${p.name}</div>
            </div>
        `;

        div.onclick = () => {
            searchInput.value = p.name;
            dropdown.style.display = 'none';
            renderProduse([p]); 
        };

        dropdown.appendChild(div);
    });

    dropdown.style.display = 'block';
}

// ascunde dropdown daca click in afara lui
document.addEventListener('click', (e) => {
    const container = document.querySelector('.search-container');
    if (container && !container.contains(e.target) && dropdown) {
        dropdown.style.display = 'none';
    }
});

// functie de filtrare
async function applyFilters() {
    const minPrice = document.getElementById('input-min').value;
    const maxPrice = document.getElementById('input-max').value;

    const stockStatus = document.getElementById('stock-select').value;
    const container = document.getElementById('container-produse');

    const params = new URLSearchParams();
    if (minPrice) params.append('minPrice', minPrice);
    if (maxPrice) params.append('maxPrice', maxPrice);
    if (stockStatus !== "") params.append('inStock', stockStatus);

    container.innerHTML = '<div class="text-center w-100 mt-5"><div class="spinner-border text-primary"></div></div>';

    try {
        const response = await fetch(`${URL_PRODUSE}/filter?${params.toString()}`);

        const produse = await response.json();
        renderProduse(produse);
    } catch (err) {
        console.error(err);
    }
}

// resetare UI
function resetFiltersUI() {
    priceSlider.noUiSlider.set([0, 200]); // resetare slider
    document.getElementById('stock-select').value = '';

    // Resetare inputuri 
    inputMin.value = 0;
    inputMax.value = 200;

    const btnToate = document.querySelector('.category-btn');
    resetFiltru(btnToate);
}