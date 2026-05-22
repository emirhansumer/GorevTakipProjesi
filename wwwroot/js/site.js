// Görev Takip Sistemi — global JS helper'lar
// SweetAlert2 üzerine ince bir sarmalayıcı

window.GorevTakip = (function () {
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

    return {
        toast: toast,
        silOnayi: silOnayi
    };
})();
