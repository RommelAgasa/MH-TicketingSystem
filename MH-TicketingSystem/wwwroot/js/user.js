function Delete(id) {
    // Use SweetAlert2 for confirmation
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel',
        reverseButtons: true
    }).then((result) => {
        if (result.value) {
            // Proceed with the AJAX request if confirmed
            $.ajax({
                url: `/User/Delete?id=${id}`,
                type: 'post',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message); // Show success message
                        setTimeout(function () {
                            location.reload();
                        }, 3000)
                    } else {
                        toastr.error(response.message); // Show error message
                    }
                },
                error: function () {
                    toastr.error('Unable to delete the data.'); // Show error message
                }
            });
        }
    });
}
