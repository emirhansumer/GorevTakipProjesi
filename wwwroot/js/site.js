// Görev Takip Sistemi — global JS helper'lar
// SweetAlert2 üzerine ince bir sarmalayıcı

window.GorevTakip = (function () {
    // Anti-forgery token meta tag'ten okur
    function csrfToken() {
        var meta = document.querySelector('meta[name="request-verification-token"]');
        return meta ? meta.content : '';
    }

    // Sağ üstte beliren küçük bildirim (toast)
    function toast(icon, title) {
        Swal.fire({
            toast: true,
            position: 'top-end',
            icon: icon,            // 'success' | 'error' | 'warning' | 'info' | 'question'
            title: title,
            showConfirmButton: false,
            timer: 3000,
            timerProgressBar: true,
            customClass: { popup: 'shadow-sm' }
        });
    }

    // Silme onayı modal'ı — kullanıcı onaylarsa formu submit eder
    function silOnayi(formId, opt) {
        opt = opt || {};
        Swal.fire({
            title: opt.title || 'Emin misin?',
            text: opt.text || 'Bu işlem geri alınamaz.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: opt.confirmText || 'Evet, sil',
            cancelButtonText: 'İptal',
            reverseButtons: true,
            focusCancel: true
        }).then(function (result) {
            if (result.isConfirmed) {
                var form = document.getElementById(formId);
                if (form) form.submit();
            }
        });
    }

    // Hızlı kategori ekleme — modal aç, AJAX POST, callback'i çağır
    function yeniKategoriEkle(onSuccess) {
        var paletteHtml = '';
        var renkler = ['#6366f1', '#ec4899', '#10b981', '#f59e0b', '#ef4444', '#06b6d4', '#8b5cf6', '#f97316', '#14b8a6', '#64748b'];
        renkler.forEach(function (r, i) {
            paletteHtml += '<button type="button" class="renk-secenek ' + (i === 0 ? 'aktif' : '') + '" data-renk="' + r + '" style="background:' + r + ';">' + (i === 0 ? '<i class="bi bi-check-lg"></i>' : '') + '</button>';
        });

        Swal.fire({
            title: 'Yeni Kategori',
            html: '<div class="text-start">' +
                  '  <label class="form-label small fw-semibold">Kategori Adı</label>' +
                  '  <input id="swalKategoriAd" class="form-control mb-3" placeholder="Örn: Okul" autocomplete="off" />' +
                  '  <label class="form-label small fw-semibold">Renk</label>' +
                  '  <div class="renk-paleti d-flex flex-wrap gap-2">' + paletteHtml + '</div>' +
                  '  <input id="swalKategoriRenk" type="hidden" value="#6366f1" />' +
                  '</div>',
            showCancelButton: true,
            confirmButtonText: 'Kaydet',
            cancelButtonText: 'İptal',
            confirmButtonColor: '#6366f1',
            reverseButtons: true,
            focusConfirm: false,
            didOpen: function () {
                var input = document.getElementById('swalKategoriAd');
                if (input) input.focus();
                document.querySelectorAll('.swal2-popup .renk-secenek[data-renk]').forEach(function (b) {
                    b.addEventListener('click', function () {
                        document.getElementById('swalKategoriRenk').value = b.dataset.renk;
                        document.querySelectorAll('.swal2-popup .renk-secenek').forEach(function (x) {
                            x.classList.remove('aktif');
                            var ic = x.querySelector('i'); if (ic) x.removeChild(ic);
                        });
                        b.classList.add('aktif');
                        var ic = document.createElement('i');
                        ic.className = 'bi bi-check-lg';
                        b.appendChild(ic);
                    });
                });
            },
            preConfirm: function () {
                var ad = (document.getElementById('swalKategoriAd').value || '').trim();
                var renk = document.getElementById('swalKategoriRenk').value;
                if (ad.length < 2) {
                    Swal.showValidationMessage('Kategori adı en az 2 karakter olmalı');
                    return false;
                }
                var formData = new FormData();
                formData.append('Ad', ad);
                formData.append('Renk', renk);
                formData.append('__RequestVerificationToken', csrfToken());
                return fetch('/Kategori/HizliEkle', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin'
                }).then(function (r) {
                    return r.json().then(function (data) {
                        if (!r.ok || !data.ok) {
                            var mesaj = data.message || (data.errors ? Object.values(data.errors).flat().join(', ') : 'Kayıt başarısız');
                            Swal.showValidationMessage(mesaj);
                            return false;
                        }
                        return data;
                    });
                }).catch(function () {
                    Swal.showValidationMessage('Sunucu hatası, tekrar dene');
                    return false;
                });
            }
        }).then(function (result) {
            if (result.isConfirmed && result.value && result.value.id) {
                toast('success', 'Kategori eklendi');
                if (typeof onSuccess === 'function') onSuccess(result.value);
            }
        });
    }

    // Inline kategori değiştirme — chip'e tıklayınca modal aç, yeni kategori seç
    function gorevKategoriDegistir(gorevId, mevcutKategoriId, kategoriler, onSuccess) {
        var html = '<div class="text-start">';
        html += '<label class="form-label small fw-semibold">Yeni kategori seç</label>';
        html += '<select id="swalKategoriDropdown" class="form-select">';
        html += '<option value="" ' + (!mevcutKategoriId ? 'selected' : '') + '>— Kategorisiz —</option>';
        (kategoriler || []).forEach(function (k) {
            var sel = String(k.id) === String(mevcutKategoriId) ? 'selected' : '';
            html += '<option value="' + k.id + '" ' + sel + '>' + k.ad + '</option>';
        });
        html += '</select>';
        html += '<small class="text-muted d-block mt-2"><i class="bi bi-info-circle me-1"></i>Sayfa yenilenmeyecek, değişiklik anında uygulanır.</small>';
        html += '</div>';

        Swal.fire({
            title: 'Kategori Değiştir',
            html: html,
            showCancelButton: true,
            confirmButtonText: 'Uygula',
            cancelButtonText: 'İptal',
            confirmButtonColor: '#6366f1',
            reverseButtons: true,
            focusConfirm: false,
            preConfirm: function () {
                var sel = document.getElementById('swalKategoriDropdown');
                var deger = sel.value;
                var formData = new FormData();
                formData.append('gorevId', gorevId);
                if (deger) formData.append('kategoriId', deger);
                formData.append('__RequestVerificationToken', csrfToken());
                return fetch('/Gorev/KategoriDegistir', {
                    method: 'POST',
                    body: formData,
                    credentials: 'same-origin'
                }).then(function (r) {
                    return r.json().then(function (data) {
                        if (!r.ok || !data.ok) {
                            Swal.showValidationMessage(data.message || 'Güncellenemedi');
                            return false;
                        }
                        return data;
                    });
                }).catch(function () {
                    Swal.showValidationMessage('Sunucu hatası');
                    return false;
                });
            }
        }).then(function (result) {
            if (result.isConfirmed && result.value && result.value.ok) {
                toast('success', 'Kategori güncellendi');
                if (typeof onSuccess === 'function') onSuccess(result.value);
            }
        });
    }

    return {
        toast: toast,
        silOnayi: silOnayi,
        yeniKategoriEkle: yeniKategoriEkle,
        gorevKategoriDegistir: gorevKategoriDegistir,
        csrfToken: csrfToken
    };
})();
