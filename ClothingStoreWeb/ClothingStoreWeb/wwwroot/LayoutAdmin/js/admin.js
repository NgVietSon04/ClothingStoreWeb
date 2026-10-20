(function () {
    'use strict';

    // ---------- Modal xác nhận xóa dùng chung ----------
    var modalEl = document.getElementById('deleteModal');
    if (modalEl && window.bootstrap) {
        var modal = new bootstrap.Modal(modalEl);
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('[data-delete-url]');
            if (!btn) return;
            document.getElementById('deleteForm').action = btn.getAttribute('data-delete-url');
            document.getElementById('deleteName').textContent = btn.getAttribute('data-delete-name') || '';
            modal.show();
        });
    }

    // ---------- Sidebar mobile ----------
    var btnSidebar = document.getElementById('btnSidebar');
    if (btnSidebar) {
        btnSidebar.addEventListener('click', function () {
            document.getElementById('sidebar').classList.toggle('open');
        });
    }

    // ---------- Xem trước ảnh khi chọn file ----------
    document.querySelectorAll('.img-input').forEach(function (input) {
        input.addEventListener('change', function () {
            var img = document.getElementById(input.getAttribute('data-preview'));
            if (img && input.files && input.files[0]) {
                img.src = URL.createObjectURL(input.files[0]);
                img.style.opacity = 1;
            }
        });
    });

    // ---------- Tự ẩn thông báo thành công ----------
    document.querySelectorAll('.alert.auto-dismiss').forEach(function (el) {
        setTimeout(function () {
            if (window.bootstrap) bootstrap.Alert.getOrCreateInstance(el).close();
        }, 4000);
    });

    // ---------- Bảng dòng hàng động (đơn hàng / phiếu nhập) ----------
    var tbody = document.getElementById('itemRows');
    if (!tbody) return;

    var table = tbody.closest('table');
    var mode = table.getAttribute('data-mode'); // 'order' | 'po'
    var tpl = document.getElementById('rowTemplate');
    var fmt = function (n) { return new Intl.NumberFormat('vi-VN').format(Math.round(n)) + ' ₫'; };

    function reindex() {
        tbody.querySelectorAll('tr').forEach(function (tr, i) {
            tr.querySelectorAll('[data-field]').forEach(function (el) {
                el.name = 'Items[' + i + '].' + el.getAttribute('data-field');
            });
        });
        calc();
    }

    function calc() {
        var sub = 0;
        tbody.querySelectorAll('tr').forEach(function (tr) {
            var q = parseFloat(tr.querySelector('.qty').value) || 0;
            var p = parseFloat(tr.querySelector('.price').value) || 0;
            var dEl = tr.querySelector('.disc');
            var d = dEl ? (parseFloat(dEl.value) || 0) : 0;
            var line = q * p * (1 - d / 100);
            tr.querySelector('.line-total').textContent = fmt(line);
            sub += line;
        });
        var subEl = document.getElementById('sumSub');
        if (subEl) subEl.textContent = fmt(sub);
        var odEl = document.getElementById('OrderDiscount');
        var od = odEl ? (parseFloat(odEl.value) || 0) : 0;
        var totEl = document.getElementById('sumTotal');
        if (totEl) totEl.textContent = fmt(sub * (1 - od / 100));
    }

    document.getElementById('btnAddRow').addEventListener('click', function () {
        tbody.appendChild(tpl.content.firstElementChild.cloneNode(true));
        reindex();
    });

    tbody.addEventListener('click', function (e) {
        var btn = e.target.closest('.btn-remove');
        if (!btn) return;
        var tr = btn.closest('tr');
        if (tbody.children.length > 1) {
            tr.remove();
        } else {
            tr.querySelectorAll('select').forEach(function (s) { s.value = ''; });
            tr.querySelectorAll('input:not([type=hidden])').forEach(function (i) { i.value = i.classList.contains('qty') ? 1 : 0; });
        }
        reindex();
    });

    tbody.addEventListener('change', function (e) {
        if (e.target.classList.contains('variant-select')) {
            var opt = e.target.selectedOptions[0];
            var tr = e.target.closest('tr');
            if (opt && opt.value) {
                if (mode === 'order') {
                    tr.querySelector('.price').value = opt.getAttribute('data-price') || 0;
                    var d = tr.querySelector('.disc');
                    if (d) d.value = opt.getAttribute('data-discount') || 0;
                }
            }
        }
        calc();
    });
    tbody.addEventListener('input', calc);

    var odInput = document.getElementById('OrderDiscount');
    if (odInput) odInput.addEventListener('input', calc);

    reindex();
})();
