/* UniManage Custom Interactions */

$(document).ready(function () {
    // 1. Auto-dismiss success alerts
    setTimeout(function () {
        $(".alert-success").fadeOut(1000, function () {
            $(this).remove();
        });
    }, 4000);

    // 2. SweetAlert2 Confirmation Dialogs
    $(document).on("click", ".btn-delete-confirm", function (e) {
        e.preventDefault();
        var form = $(this).closest("form");
        var title = $(this).data("confirm-title") || "Are you sure?";
        var text = $(this).data("confirm-text") || "You won't be able to revert this!";

        Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#EF4444',
            cancelButtonColor: '#6B7280',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                form.submit();
            }
        });
    });

    // 3. File Upload Dropzone handling
    $(document).on("dragover", ".dropzone-custom", function (e) {
        e.preventDefault();
        $(this).addClass("dragover");
    });

    $(document).on("dragleave", ".dropzone-custom", function (e) {
        e.preventDefault();
        $(this).removeClass("dragover");
    });

    $(document).on("drop", ".dropzone-custom", function (e) {
        e.preventDefault();
        $(this).removeClass("dragover");
        var files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            var input = $(this).find("input[type='file']");
            input[0].files = files;
            updateFileInfo($(this), files[0]);
        }
    });

    $(document).on("change", ".dropzone-custom input[type='file']", function () {
        if (this.files.length > 0) {
            updateFileInfo($(this).closest(".dropzone-custom"), this.files[0]);
        }
    });

    function updateFileInfo(dropzone, file) {
        var infoArea = dropzone.find(".file-info-area");
        if (infoArea.length === 0) {
            dropzone.append('<div class="file-info-area mt-3"></div>');
            infoArea = dropzone.find(".file-info-area");
        }
        
        var sizeMB = (file.size / (1024 * 1024)).toFixed(2);
        if (sizeMB > 10) {
            Swal.fire("Error", "File exceeds 10MB limit.", "error");
            dropzone.find("input[type='file']").val("");
            infoArea.empty();
            return;
        }

        infoArea.html(`
            <div class="p-3 bg-light rounded text-left d-flex align-items-center">
                <i class="fa-solid fa-file-circle-check fa-2x mr-3 text-primary"></i>
                <div class="flex-grow-1">
                    <div class="font-weight-bold text-dark">${file.name}</div>
                    <div class="small text-muted">${sizeMB} MB</div>
                </div>
            </div>
        `);
    }

    // 4. Password Strength Meter
    $("#Password").on("keyup", function () {
        var val = $(this).val();
        var strength = 0;
        if (val.length >= 8) strength++;
        if (val.match(/[A-Z]/)) strength++;
        if (val.match(/[a-z]/)) strength++;
        if (val.match(/[0-9]/)) strength++;
        if (val.match(/[^A-Za-z0-9]/)) strength++;

        var meter = $("#password-strength-meter");
        var label = $("#password-strength-label");
        var colors = ["#EF4444", "#F59E0B", "#F59E0B", "#10B981", "#10B981"];
        var texts = ["Weak", "Fair", "Fair", "Good", "Strong"];

        meter.css("width", (strength * 20) + "%");
        meter.css("background-color", colors[strength - 1] || "#E5E7EB");
        label.text(texts[strength - 1] || "");
    });

    // 5. Sidebar Toggle
    $("#sidebar-toggle").on("click", function () {
        $("body").toggleClass("sidebar-collapsed");
        var isCollapsed = $("body").hasClass("sidebar-collapsed");
        localStorage.setItem("sidebarCollapsed", isCollapsed);
    });

    if (localStorage.getItem("sidebarCollapsed") === "true") {
        $("body").addClass("sidebar-collapsed");
    }

    // 6. Mobile Sidebar Toggle
    $("#mobile-toggle").on("click", function () {
        $(".sidebar").toggleClass("sidebar-open");
    });

    // 7. Form Loading State
    $("form").on("submit", function () {
        if ($(this).valid()) {
            var btn = $(this).find("button[type='submit']");
            btn.prop("disabled", true);
            btn.html('<i class="fa-solid fa-spinner fa-spin mr-2"></i> Processing...');
        }
    });
});
