/**
 * UME Admin JavaScript
 * Custom scripts for admin dashboard
 */

// Initialize when document is ready
$(document).ready(function() {
    // Initialize DataTables with Vietnamese language
    if ($.fn.DataTable) {
        $.extend($.fn.dataTable.defaults, {
            language: {
                "decimal": "",
                "emptyTable": "Không có dữ liệu",
                "info": "Hiển thị _START_ đến _END_ của _TOTAL_ mục",
                "infoEmpty": "Hiển thị 0 đến 0 của 0 mục",
                "infoFiltered": "(lọc từ _MAX_ tổng số mục)",
                "infoPostFix": "",
                "thousands": ",",
                "lengthMenu": "Hiển thị _MENU_ mục",
                "loadingRecords": "Đang tải...",
                "processing": "Đang xử lý...",
                "search": "Tìm kiếm:",
                "zeroRecords": "Không tìm thấy kết quả phù hợp",
                "paginate": {
                    "first": "Đầu",
                    "last": "Cuối",
                    "next": "Tiếp",
                    "previous": "Trước"
                },
                "aria": {
                    "sortAscending": ": sắp xếp cột tăng dần",
                    "sortDescending": ": sắp xếp cột giảm dần"
                }
            }
        });
    }

    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();

    // Initialize popovers
    $('[data-toggle="popover"]').popover();
});

// UME Admin Utilities
const UmeAdmin = {
    // Format currency VND
    formatCurrency: function(amount) {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    },

    // Format number with thousand separators
    formatNumber: function(num) {
        return new Intl.NumberFormat('vi-VN').format(num);
    },

    // Format date
    formatDate: function(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    },

    // Format datetime
    formatDateTime: function(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    },

    // Show loading overlay
    showLoading: function() {
        Swal.fire({
            title: 'Đang xử lý...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    },

    // Hide loading
    hideLoading: function() {
        Swal.close();
    },

    // Show success message
    showSuccess: function(message) {
        toastr.success(message);
    },

    // Show error message
    showError: function(message) {
        toastr.error(message);
    },

    // Show warning message
    showWarning: function(message) {
        toastr.warning(message);
    },

    // Show info message
    showInfo: function(message) {
        toastr.info(message);
    },

    // Confirm dialog
    confirm: function(title, text, callback) {
        Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#D4AF37',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Đồng ý',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    },

    // Delete confirmation
    confirmDelete: function(callback) {
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: 'Bạn có chắc chắn muốn xóa mục này? Hành động này không thể hoàn tác!',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#E53935',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    },

    // API call helper
    apiCall: function(url, method, data, successCallback, errorCallback) {
        $.ajax({
            url: url,
            type: method,
            contentType: 'application/json',
            data: data ? JSON.stringify(data) : null,
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('adminToken')
            },
            success: function(response) {
                if (successCallback) successCallback(response);
            },
            error: function(xhr, status, error) {
                if (errorCallback) {
                    errorCallback(xhr, status, error);
                } else {
                    UmeAdmin.showError('Có lỗi xảy ra: ' + error);
                }
            }
        });
    },

    // Initialize DataTable
    initDataTable: function(tableId, options = {}) {
        const defaultOptions = {
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>',
            order: [[0, 'desc']]
        };

        return $(tableId).DataTable({...defaultOptions, ...options});
    },

    // Initialize Flatpickr
    initDatePicker: function(selector, options = {}) {
        const defaultOptions = {
            dateFormat: 'd/m/Y',
            locale: 'vn'
        };

        return flatpickr(selector, {...defaultOptions, ...options});
    },

    // Initialize DateRange Picker
    initDateRangePicker: function(selector, options = {}) {
        const defaultOptions = {
            mode: 'range',
            dateFormat: 'd/m/Y',
            locale: 'vn'
        };

        return flatpickr(selector, {...defaultOptions, ...options});
    },

    // Get status badge HTML
    getStatusBadge: function(status) {
        const statusMap = {
            'Pending': { class: 'badge-pending', text: 'Chờ xử lý' },
            'Confirmed': { class: 'badge-confirmed', text: 'Đã xác nhận' },
            'InProgress': { class: 'badge-info', text: 'Đang thực hiện' },
            'Completed': { class: 'badge-completed', text: 'Hoàn thành' },
            'Cancelled': { class: 'badge-cancelled', text: 'Đã hủy' },
            'Active': { class: 'badge-success', text: 'Hoạt động' },
            'Inactive': { class: 'badge-secondary', text: 'Không hoạt động' },
            'Paid': { class: 'badge-success', text: 'Đã thanh toán' },
            'Unpaid': { class: 'badge-warning', text: 'Chưa thanh toán' },
            'Refunded': { class: 'badge-info', text: 'Đã hoàn tiền' }
        };

        const statusInfo = statusMap[status] || { class: 'badge-secondary', text: status };
        return `<span class="badge ${statusInfo.class}">${statusInfo.text}</span>`;
    },

    // Debounce function
    debounce: function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// Export for global use
window.UmeAdmin = UmeAdmin;
