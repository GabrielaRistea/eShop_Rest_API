let globalOrders = [];
let currentOrderId = 0;

document.addEventListener('DOMContentLoaded', () => {
    if (typeof isLoggedIn === 'function' && !isLoggedIn()) {
        window.location.href = 'login.html';
        return;
    }
    loadAllOrders();
});

async function loadAllOrders() {
    const tableBody = document.getElementById('all-orders-table');
    const loadingRow = `<tr><td colspan="6" class="text-center py-4"><div class="spinner-border text-primary"></div><br>Se încarcă comenzile...</td></tr>`;

    if (tableBody) tableBody.innerHTML = loadingRow;

    try {
        const headers = typeof getAuthHeaders === 'function' ? getAuthHeaders() : {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
        };

        const response = await fetch(`${API_BASE}/HistoryOrders/all-orders`, {
            headers: headers
        });

        if (!response.ok) {
            if (response.status === 401) {
                alert("Sesiunea a expirat.");
                window.location.href = 'login.html';
                return;
            }
            throw new Error("Eroare server: " + response.status);
        }

        const rawOrders = await response.json();

        if (!rawOrders || rawOrders.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="6" class="text-center py-4">Nu există comenzi în sistem.</td></tr>`;
            return;
        }

        // MAPARE DATE 
        globalOrders = rawOrders.map(o => {
            return {
                id: o.orderId || o.OrderId || o.orderID || o.id || 0,
                userName: o.customerName || o.CustomerName || 'Client',
                userEmail: o.customerEmail || o.CustomerEmail || '-',
                phoneNumber: o.phoneNumber || o.PhoneNumber || 'Nu are telefon',

                orderDate: o.orderDate || o.OrderDate || new Date(),
                totalAmount: o.totalAmount || o.TotalAmount || 0,
                status: o.status || o.Status || o.statusOrder || 'Necunoscut',
                address: o.address || o.Address || 'Nespecificat'

            };
        });

        globalOrders.sort((a, b) => b.id - a.id);

        //  GENERARE TABEL 
        tableBody.innerHTML = '';

        globalOrders.forEach(order => {
            let dateStr = '-';
            try {
                dateStr = new Date(order.orderDate).toLocaleDateString('ro-RO', {
                    day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit'
                });
            } catch (e) { }

            let badgeClass = 'bg-secondary';
            const s = (order.status || '').toLowerCase();
            if (s.includes('platit') || s.includes('finalizata')) badgeClass = 'bg-success';
            else if (s.includes('asteptare') || s.includes('procesare')) badgeClass = 'bg-warning text-dark';
            else if (s.includes('anulata')) badgeClass = 'bg-danger';

            const row = `
                <tr>
                    <td class="fw-bold">#${order.id}</td>
                    <td>${dateStr}</td>
                    <td>
                        <div class="fw-bold text-dark">${order.userName}</div>
                        
                    </td>
                    <td class="fw-bold text-primary">${order.totalAmount.toFixed(2)} RON</td>
                    <td><span class="badge ${badgeClass}">${order.status}</span></td>
                    <td>
                        <button class="btn btn-sm btn-outline-primary" 
                                onclick="openOrderById(${order.id})">
                            <i class="bi bi-gear"></i> Gestionează
                        </button>
                    </td>
                </tr>
            `;
            tableBody.insertAdjacentHTML('beforeend', row);
        });

    } catch (err) {
        console.error(err);
        if (tableBody) tableBody.innerHTML = `<tr><td colspan="6" class="text-danger text-center">Eroare: ${err.message}</td></tr>`;
    }
}

//  DESCHIDE MODAL 
window.openOrderById = function (id) {
    const order = globalOrders.find(o => o.id === id);
    if (!order) return;

    currentOrderId = order.id;

    // populare date de baza
    document.getElementById('modal-order-id').innerText = '#' + order.id;
    document.getElementById('modal-client').innerText = `${order.userName}`;

    const addressElement = document.getElementById('modal-address');
    addressElement.innerHTML = `
        ${order.address} <br>
        <span class="text-success fw-bold"><i class="bi bi-telephone-fill me-1"></i> ${order.phoneNumber}</span>
    `;

    document.getElementById('modal-total').innerText = order.totalAmount.toFixed(2);

    // setare status
    const statusSelect = document.getElementById('modal-new-status');
    statusSelect.value = order.status;
    if (!statusSelect.value) {
        for (let i = 0; i < statusSelect.options.length; i++) {
            if (statusSelect.options[i].value.toLowerCase() === order.status.toLowerCase()) {
                statusSelect.selectedIndex = i;
                break;
            }
        }
    }

    const itemsList = document.getElementById('modal-items-list');
    if (itemsList) {
        itemsList.innerHTML = '';.
        itemsList.style.display = 'none'; 
    }

    const modal = new bootstrap.Modal(document.getElementById('orderModal'));
    modal.show();
}

// ACTUALIZARE STATUS 
window.updateOrderStatus = async function () {
    const newStatus = document.getElementById('modal-new-status').value;

    if (!confirm(`Schimbi statusul în "${newStatus}"?`)) return;

    try {
        const headers = typeof getAuthHeaders === 'function' ? getAuthHeaders() : {
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
            'Content-Type': 'application/json'
        };
        headers['Content-Type'] = 'application/json';

        const response = await fetch(`${API_BASE}/HistoryOrders/${currentOrderId}/status`, {
            method: 'PATCH',
            headers: headers,
            body: JSON.stringify({ newStatus: newStatus })
        });

        if (response.ok) {
            alert("Status actualizat!");
            const modalEl = document.getElementById('orderModal');
            const modalInstance = bootstrap.Modal.getInstance(modalEl);
            modalInstance.hide();
            loadAllOrders();
        } else {
            const err = await response.json();
            alert("Eroare: " + (err.message || "Eroare necunoscută"));
        }
    } catch (e) {
        console.error(e);
        alert("Eroare de conexiune.");
    }
}