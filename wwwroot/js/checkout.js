document.addEventListener('DOMContentLoaded', async () => {
    if (!isLoggedIn()) {
        window.location.href = 'login.html';
        return;
    }
    await loadOrderSummary();
});

async function loadOrderSummary() {
    try {
        const response = await fetch(`${API_BASE}/Orders`, {
            headers: getAuthHeaders()
        });

        if (!response.ok) throw new Error("Nu s-a putut încărca coșul.");

        const cart = await response.json();
        renderSummary(cart);

    } catch (err) {
        console.error(err);
        document.getElementById('order-summary-list').innerHTML =
            `<li class="list-group-item text-danger text-center">Eroare la încărcare sumar.</li>`;
    }
}

function renderSummary(cart) {
    const listContainer = document.getElementById('order-summary-list');
    const subtotalEl = document.getElementById('summary-subtotal');
    const totalEl = document.getElementById('summary-total');

    // daca cosul e gol
    if (!cart.items || cart.items.length === 0) {
        alert("Coșul este gol!");
        window.location.href = 'products.html';
        return;
    }

    listContainer.innerHTML = '';

    // generare html pentru fiecare produs
    cart.items.forEach(item => {
        let imgSrc = 'https://dummyimage.com/60x60/dee2e6/6c757d.jpg';
        if (item.productImage) {
            imgSrc = `data:image/jpeg;base64,${item.productImage}`;
        }

        const li = document.createElement('li');
        li.className = 'list-group-item d-flex justify-content-between align-items-center py-3';
        li.innerHTML = `
            <div class="d-flex align-items-center">
                <img src="${imgSrc}" class="summary-img me-3" alt="${item.productName}">
                <div>
                    <h6 class="my-0 small fw-bold text-truncate" style="max-width: 150px;">${item.productName}</h6>
                    <small class="text-muted">Cantitate: ${item.quantity}</small>
                </div>
            </div>
            <span class="text-muted small fw-bold">${item.subtotal} RON</span>
        `;
        listContainer.appendChild(li);
    });

    // actualizare totaluri
    subtotalEl.textContent = `${cart.totalAmount} RON`;
    totalEl.textContent = `${cart.totalAmount} RON`;
}

// gestionarea formularului 
document.getElementById('checkout-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const address = document.getElementById('address').value;
    const phone = document.getElementById('phone').value;
    const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked').value;
    const btn = e.target.querySelector('button[type="submit"]');

    const originalText = btn.innerHTML;
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Se procesează...';

    try {
        const response = await fetch(`${API_BASE}/Orders/checkout`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({
                shippingAddress: address,
                phoneNumber: phone,
                paymentMethod: paymentMethod
            })
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data.message || "Eroare la plasarea comenzii.");
        }

        if (data.redirectToStripe && data.stripeUrl) {
            window.location.href = data.stripeUrl;
        } else {
            // caz ramburs
            window.location.href = `order-confirmation.html?orderId=${data.orderId}`;
        }

    } catch (err) {
        alert(`❌ Eroare: ${err.message}`);
        btn.disabled = false;
        btn.innerHTML = originalText;
    }
});