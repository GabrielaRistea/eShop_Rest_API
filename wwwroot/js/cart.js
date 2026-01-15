const URL_CART = `${API_BASE}/Orders`;
const URL_CHECKOUT = `${API_BASE}/Orders/checkout`;

document.addEventListener('DOMContentLoaded', () => {
    if (!isLoggedIn()) {
        window.location.href = 'login.html';
        return;
    }
    loadCart();
});

// incarcare cos
async function loadCart() {
    try {
        const response = await fetch(URL_CART, {
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) window.location.href = 'login.html';
            throw new Error("Eroare la încărcarea coșului.");
        }

        const cartData = await response.json();
        renderCart(cartData);

    } catch (err) {
        console.error(err);
        document.getElementById('loading-msg').innerHTML = `<p class="text-danger">Eroare: ${err.message}</p>`;
    }
}

function renderCart(cartData) {
    document.getElementById('loading-msg').classList.add('d-none');

    // verificam daca cosul e gol
    if (!cartData.items || cartData.items.length === 0) {
        document.getElementById('empty-cart-msg').classList.remove('d-none');
        document.getElementById('cart-content').classList.add('d-none');
        return;
    }

    document.getElementById('empty-cart-msg').classList.add('d-none');
    document.getElementById('cart-content').classList.remove('d-none');

    const tbody = document.getElementById('cart-items-body');
    tbody.innerHTML = '';

    cartData.items.forEach(item => {
        let imgSrc = 'img/no-image.jpg';
        if (item.productImage) {
            imgSrc = `data:image/jpeg;base64,${item.productImage}`;
        }

        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td class="ps-4">
                <div class="d-flex align-items-center">
                    <img src="${imgSrc}" class="cart-img me-3" alt="${item.productName}">
                    <div>
                        <h6 class="mb-0 fw-bold">${item.productName}</h6>
                    </div>
                </div>
            </td>
            <td>${item.pricePerUnit} RON</td>
            <td>
                <div class="input-group input-group-sm quantity-control">
                    <button class="btn btn-outline-secondary" onclick="updateQuantity(${item.productId}, false)">-</button>
                    <input type="text" class="form-control text-center bg-white" value="${item.quantity}" readonly>
                    <button class="btn btn-outline-secondary" onclick="updateQuantity(${item.productId}, true)">+</button>
                </div>
            </td>
            <td class="fw-bold">${item.subtotal} RON</td>
        `;
        tbody.appendChild(tr);
    });

    document.getElementById('cart-total').textContent = `${cartData.totalAmount} RON`;
}

async function updateQuantity(productId, increase) {
    try {
        const url = `${URL_CART}/update-quantity?productId=${productId}&increase=${increase}`;

        const response = await fetch(url, {
            method: 'POST',
            headers: getAuthHeaders()
        });

        if (response.ok) {
            loadCart(); // reincarca cosul pentru a vedea noile valori
        } else {
            alert("Eroare la actualizarea cantității.");
        }
    } catch (err) {
        console.error(err);
    }
}

// golire cos
async function clearCart() {
    if (!confirm("Sigur vrei să golești tot coșul?")) return;

    try {
        const response = await fetch(`${URL_CART}/clear`, {
            method: 'DELETE',
            headers: getAuthHeaders()
        });

        if (response.ok) loadCart();
        else alert("Eroare la ștergere.");
    } catch (err) {
        console.error(err);
    }
}

// checkout
document.getElementById('checkout-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const address = document.getElementById('address').value;
    const phone = document.getElementById('phone').value;
    const paymentMethod = document.querySelector('input[name="paymentMethod"]:checked').value;
    const errorDiv = document.getElementById('checkout-error');
    errorDiv.textContent = "";

    const btn = e.target.querySelector('button');
    btn.disabled = true;
    btn.textContent = "Se procesează...";

    try {
        const response = await fetch(URL_CHECKOUT, {
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

        // daca e card, redirectam la stripe
        if (data.redirectToStripe && data.stripeUrl) {
            window.location.href = data.stripeUrl;
        } else {
            // daca e ramburs
            alert("Comanda a fost plasată cu succes! ID Comandă: " + data.orderId);
            window.location.href = 'index.html'; // sau o pagina de succes
        }

    } catch (err) {
        errorDiv.textContent = err.message;
        btn.disabled = false;
        btn.textContent = "Finalizează Comanda ✅";
    }
});