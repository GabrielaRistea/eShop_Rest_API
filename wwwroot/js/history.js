document.addEventListener('DOMContentLoaded', () => {
    if (!isLoggedIn()) {
        window.location.href = 'login.html';
        return;
    }
    loadOrderHistory();
});

async function loadOrderHistory() {
    const container = document.getElementById('orders-container');
    const loading = document.getElementById('loading-history');
    const noOrders = document.getElementById('no-orders');

    if (!container || !loading) return;

    try {
        const url = `${API_BASE}/HistoryOrders/my-orders`;

        const response = await fetch(url, {
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                logout();
                return;
            }
            throw new Error(`Server status: ${response.status}`);
        }

        const rawOrders = await response.json();
        processOrders(rawOrders, container, loading, noOrders);

    } catch (err) {
        console.error("[Fetch Error]", err);
        if (loading) loading.classList.add('d-none');
        container.innerHTML = `
            <div class="col-12 text-center mt-5">
                <div class="alert alert-danger d-inline-block px-4">
                    <i class="bi bi-wifi-off me-2"></i> Eroare conexiune: ${err.message}
                </div>
            </div>`;
    }
}

function processOrders(orders, container, loading, noOrders) {
    if (loading) loading.classList.add('d-none');

    if (!orders || !Array.isArray(orders) || orders.length === 0) {
        if (noOrders) noOrders.classList.remove('d-none');
        return;
    }

    // normalizare sigura a comenzilor
    const safeOrders = orders.map(o => {
        if (!o) return null;
        return {
            id: o.orderID || o.OrderID || o.id || o.Id || 0,
            orderDate: o.orderDate || o.OrderDate || new Date().toISOString(),
            totalAmount: o.totalAmount || o.TotalAmount || 0,
            statusOrder: o.statusOrder || o.StatusOrder || 'Necunoscut',
            paymentMethod: o.paymentMethod || o.PaymentMethod || '-',
            rawItems: o.orderItems || o.OrderItems || []
        };
    }).filter(o => o !== null);

    // sortare: cele mai noi primele
    safeOrders.sort((a, b) => b.id - a.id);

    container.innerHTML = '';

    safeOrders.forEach(order => {
        // procesare data 
        let dateStr = '-', timeStr = '-';
        try {
            const dateObj = new Date(order.orderDate);
            dateStr = dateObj.toLocaleDateString('ro-RO', { day: 'numeric', month: 'long', year: 'numeric' });
            timeStr = dateObj.toLocaleTimeString('ro-RO', { hour: '2-digit', minute: '2-digit' });
        } catch (e) { }

        // procesare produse si imagine
        const safeItems = order.rawItems.map(i => {
            // gasim obiectul produs 
            const prodObj = i.product || i.Product || {};

            // extragem imaginea bruta
            let rawImg = prodObj.productImage || prodObj.ProductImage || '';
            let finalImg = 'https://via.placeholder.com/60?text=No+Img'; 

            if (rawImg) {
                if (!rawImg.includes('.') && rawImg.length > 50) {
                    // verificam daca are deja header-ul
                    if (rawImg.startsWith('data:image')) {
                        finalImg = rawImg;
                    } else {
                        // adaugam header-ul pentru ca browserul sa stie ca e o poza
                        finalImg = `data:image/jpeg;base64,${rawImg}`;
                    }
                }
                // daca e link extern
                else if (rawImg.startsWith('http')) {
                    finalImg = rawImg;
                }
                // daca e nume de fisier local
                else {
                    finalImg = `images/${rawImg}`;
                }
            }

            return {
                quantity: i.quantity || i.Quantity || 0,
                productName: prodObj.name || prodObj.Name || i.productName || i.ProductName || 'Produs necunoscut',
                image: finalImg,
                price: i.price || i.Price || prodObj.price || prodObj.Price || 0
            };
        });

        // calcul total (daca lipseste din comanda)
        let total = order.totalAmount;
        if ((!total || total === 0) && safeItems.length > 0) {
            total = safeItems.reduce((sum, item) => sum + (item.price * item.quantity), 0);
        }
        total = parseFloat(total).toFixed(2);

        let statusClass = 'bg-secondary';
        let statusIcon = '<i class="bi bi-circle"></i>';
        const s = (order.statusOrder || '').toLowerCase();

        if (s.includes('platit') || s.includes('finalizata')) {
            statusClass = 'bg-success';
            statusIcon = '<i class="bi bi-check-circle-fill"></i>';
        } else if (s.includes('asteptare') || s.includes('procesare')) {
            statusClass = 'bg-warning text-dark';
            statusIcon = '<i class="bi bi-clock-fill"></i>';
        } else if (s.includes('anulata')) {
            statusClass = 'bg-danger';
            statusIcon = '<i class="bi bi-x-circle-fill"></i>';
        }

        let itemsHtml = '<p class="text-muted small fst-italic">Fără detalii produse.</p>';
        if (safeItems.length > 0) {
            itemsHtml = safeItems.map(item => `
                <div class="d-flex justify-content-between align-items-center border-bottom py-2">
                    <div class="d-flex align-items-center">
                        <div class="me-3 position-relative">
                            <img src="${item.image}" alt="img" 
                                 class="rounded border bg-white" 
                                 style="width: 50px; height: 50px; object-fit: cover;">
                            <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-primary" 
                                  style="font-size: 0.7rem;">
                                ${item.quantity}
                            </span>
                        </div>
                        
                        <div>
                            <div class="fw-bold text-dark small">${item.productName}</div>
                            <small class="text-muted">Preț unitar: ${item.price} RON</small>
                        </div>
                    </div>
                    
                    <span class="fw-bold text-primary small">${(item.price * item.quantity).toFixed(2)} RON</span>
                </div>
            `).join('');
        }

        const cardHtml = `
        <div class="col-12">
            <div class="card shadow-sm mb-3 border-0" style="border-radius: 12px; overflow:hidden;">
                <div class="card-header bg-white pt-3 pb-3 px-4 d-flex flex-wrap justify-content-between align-items-center border-bottom">
                    <div>
                        <h6 class="fw-bold mb-1">Comanda #${order.id}</h6>
                        <small class="text-muted">
                            <i class="bi bi-calendar3 me-1"></i> ${dateStr} &bull; ${timeStr}
                        </small>
                    </div>
                    <div class="d-flex align-items-center gap-3">
                         <div class="text-end d-none d-sm-block">
                            <span class="d-block small text-muted text-uppercase fw-bold" style="font-size: 0.65rem;">Total</span>
                            <span class="fw-bold text-primary">${total} RON</span>
                         </div>
                        <span class="badge ${statusClass} rounded-pill py-2 px-3">
                            ${statusIcon} ${order.statusOrder}
                        </span>
                    </div>
                </div>

                <div class="card-body px-0 py-0">
                    <div class="d-sm-none d-flex justify-content-between align-items-center p-3 bg-light border-bottom">
                        <span class="fw-bold text-muted">Total</span>
                        <span class="fw-bold text-primary fs-5">${total} RON</span>
                    </div>

                    <div class="accordion accordion-flush" id="accordion-${order.id}">
                        <div class="accordion-item border-0">
                            <h2 class="accordion-header">
                                <button class="accordion-button collapsed py-3 bg-light text-secondary fw-bold small" type="button" data-bs-toggle="collapse" data-bs-target="#collapse-${order.id}">
                                    <i class="bi bi-basket3-fill me-2"></i> Vezi Produsele (${safeItems.length})
                                </button>
                            </h2>
                            <div id="collapse-${order.id}" class="accordion-collapse collapse" data-bs-parent="#accordion-${order.id}">
                                <div class="accordion-body bg-white p-3">
                                    ${itemsHtml}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                 <div class="card-footer bg-white border-top px-4 py-2">
                    <small class="text-muted d-flex align-items-center">
                        <i class="bi bi-credit-card-2-front me-2 fs-5"></i> 
                        Plată: <strong class="ms-1 text-dark">${order.paymentMethod}</strong>
                    </small>
                </div>
            </div>
        </div>
        `;
        container.insertAdjacentHTML('beforeend', cardHtml);
    });
}