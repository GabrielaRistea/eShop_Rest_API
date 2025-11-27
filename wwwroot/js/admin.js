// variabile globale pentru a sti dacă editam ceva
let isEditingCategory = false;
let isEditingProduct = false;

document.addEventListener('DOMContentLoaded', () => {
    if (!isLoggedIn()) {
        window.location.href = 'login.html';
        return;
    }

    if (!isAdmin()) {
        alert("Nu ai acces la această pagină!");
        window.location.href = 'index.html';
        return;
    }

    refreshCategories();
    refreshProducts();
});

// citire si stergere
async function refreshCategories() {
    try {
        const res = await fetch(URL_CATEGORII);
        const categorii = await res.json();

        // populează selectul de la produse
        const select = document.getElementById('select-categorie');

        // pastrare valoare selectata pentru edit
        const currentValue = select.value;
        select.innerHTML = '<option value="">Alege o categorie...</option>';

        // populeaza lista categorii
        const listaUl = document.getElementById('lista-categorii-afisare');
        listaUl.innerHTML = '';

        categorii.forEach(c => {
            // optiune dropdown
            const opt = document.createElement('option');
            opt.value = c.categoryID;
            opt.textContent = c.name;
            select.appendChild(opt);

            // item in lista
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span>${c.name}</span>
                <div>
                    <button class="btn btn-sm btn-outline-warning me-1" onclick="startEditCategory(${c.categoryID}, '${c.name}')">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteCategory(${c.categoryID})">Delete</button>
                </div>
            `;
            listaUl.appendChild(li);
        });

        if (currentValue) select.value = currentValue; // restauram selectia

    } catch (err) {
        console.error("Eroare categorii:", err);
    }
}

// creare si update
// form submit
document.getElementById('form-categorie').addEventListener('submit', async (e) => {
    e.preventDefault();
    const numeCat = document.getElementById('cat-nume').value;
    const msgDiv = document.getElementById('msg-cat');
    const editId = document.getElementById('edit-cat-id').value;

    try {
        let url = URL_CATEGORII;
        let method = 'POST';
        let body = { name: numeCat };

        // id categorie
        if (isEditingCategory && editId) {
            url = `${URL_CATEGORII}/${editId}`;
            method = 'PUT';
            body.categoryID = parseInt(editId);
        }

        const response = await fetch(url, {
            method: method,
            headers: getAuthHeaders(), 
            body: JSON.stringify(body)
        });

        if (response.ok) {
            msgDiv.innerHTML = `<div class="alert alert-success">${isEditingCategory ? 'Actualizat' : 'Adăugat'} cu succes!</div>`;
            resetFormCategory();
            refreshCategories();
            setTimeout(() => msgDiv.innerHTML = '', 3000);
        } else {
            msgDiv.innerHTML = '<div class="alert alert-danger">Eroare la salvare.</div>';
        }
    } catch (err) {
        msgDiv.innerHTML = `<div class="alert alert-danger">${err}</div>`;
    }
});

// edit
function startEditCategory(id, name) {
    isEditingCategory = true;
    document.getElementById('edit-cat-id').value = id;
    document.getElementById('cat-nume').value = name;

    document.getElementById('btn-submit-cat').textContent = "Actualizează Categoria";
    document.getElementById('btn-submit-cat').classList.replace('btn-primary', 'btn-warning');
    document.getElementById('btn-cancel-cat').style.display = 'block';
}

// delete
async function deleteCategory(id) {
    if (!confirm('Sigur ștergi categoria? Produsele asociate s-ar putea șterge și ele!')) return;

    try {
        const res = await fetch(`${URL_CATEGORII}/${id}`, {
            method: 'DELETE',
            headers: getAuthHeaders() 
        });
        if (res.ok) refreshCategories();
        else alert('Eroare la ștergere.');
    } catch (err) { console.error(err); }
}

// reset form
function resetFormCategory() {
    isEditingCategory = false;
    document.getElementById('edit-cat-id').value = '';
    document.getElementById('form-categorie').reset();

    document.getElementById('btn-submit-cat').textContent = "Adaugă Categorie";
    document.getElementById('btn-submit-cat').classList.replace('btn-warning', 'btn-primary');
    document.getElementById('btn-cancel-cat').style.display = 'none';
}

// citire si stergere
async function refreshProducts() {
    try {
        const res = await fetch(URL_PRODUSE);
        const produse = await res.json();

        const tbody = document.getElementById('tabel-produse-body');
        tbody.innerHTML = '';

        if (produse.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" class="text-center">Nu există produse.</td></tr>';
            return;
        }

        produse.forEach(p => {
            let thumb = p.productImage
                ? `<img src="data:image/jpeg;base64,${p.productImage}" style="width:40px;height:40px;object-fit:cover;">`
                : '<span class="text-muted small">Fără</span>';

            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${thumb}</td>
                <td class="fw-bold">${p.name}</td>
                <td><span class="badge bg-secondary">${p.categoryName || 'N/A'}</span></td>
                <td>${p.price} RON</td>
                <td><small class="text-muted">Stoc: ${p.stock ?? 0}</small></td>
                <td>
                    <button class="btn btn-sm btn-outline-warning" onclick="startEditProduct(${p.id})">Edit</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="deleteProduct(${p.id})">Delete</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) { console.error(err); }
}

// creare stergere
document.getElementById('form-produs').addEventListener('submit', async (e) => {
    e.preventDefault();
    const form = document.getElementById('form-produs');
    const msgDiv = document.getElementById('msg-prod');

    const formData = new FormData(form);

    const idValue = formData.get('Id');
    if (!idValue || idValue === "") {
        formData.delete('Id');
    }


    const editId = document.getElementById('edit-prod-id').value;

    try {
        let url = URL_PRODUSE;
        let method = 'POST';

        if (isEditingProduct && editId) {
            url = `${URL_PRODUSE}/${editId}`;
            method = 'PUT';
        }

        const headers = {};
        const token = getToken();
        if (token) headers['Authorization'] = `Bearer ${token}`;

        const response = await fetch(url, {
            method: method,
            headers: headers, 
            body: formData
        });

        if (response.ok) {
            msgDiv.innerHTML = `<div class="alert alert-success">${isEditingProduct ? 'Actualizat' : 'Adăugat'} cu succes!</div>`;
            resetFormProduct();
            refreshProducts();
            setTimeout(() => msgDiv.innerHTML = '', 3000);
        } else {

            const errorJson = await response.json();
            console.error("Eroare Backend:", errorJson);

            let errorMsg = `Eroare: ${response.status}`;
            if (errorJson.errors) {
                // mesaje validare
                errorMsg = Object.values(errorJson.errors).flat().join('<br>');
            }

            msgDiv.innerHTML = `<div class="alert alert-danger">${errorMsg}</div>`;
        }
    } catch (err) {
        msgDiv.innerHTML = `<div class="alert alert-danger">${err}</div>`;
    }
});

// editare produs
async function startEditProduct(id) {
    try {
        const res = await fetch(`${URL_PRODUSE}/${id}`);
        const p = await res.json();

        isEditingProduct = true;

        // completam formularul
        document.getElementById('edit-prod-id').value = p.id;
        document.getElementById('prod-name').value = p.name;
        document.getElementById('prod-price').value = p.price;
        document.getElementById('prod-desc').value = p.description;
        document.getElementById('prod-stock').value = p.stock;

        const select = document.getElementById('select-categorie');
        select.value = p.category;

        document.getElementById('btn-submit-prod').textContent = "Actualizează Produsul";
        document.getElementById('btn-submit-prod').classList.replace('btn-success', 'btn-warning');
        document.getElementById('btn-cancel-prod').style.display = 'block';

        // scroll sus la formular
        document.getElementById('form-produs').scrollIntoView({ behavior: 'smooth' });

    } catch (err) { console.error("Nu pot încărca produsul pt editare", err); }
}

// stergere produs
async function deleteProduct(id) {
    if (!confirm('Sigur vrei să ștergi acest produs?')) return;
    try {
        const res = await fetch(`${URL_PRODUSE}/${id}`, {
            method: 'DELETE',
            headers: getAuthHeaders() 
        });
        if (res.ok) refreshProducts();
        else alert('Eroare la ștergere.');
    } catch (err) { console.error(err); }
}

// reset form produs
function resetFormProduct() {
    isEditingProduct = false;
    document.getElementById('edit-prod-id').value = '';
    document.getElementById('form-produs').reset();

    document.getElementById('btn-submit-prod').textContent = "Adaugă Produs";
    document.getElementById('btn-submit-prod').classList.replace('btn-warning', 'btn-success');
    document.getElementById('btn-cancel-prod').style.display = 'none';
}