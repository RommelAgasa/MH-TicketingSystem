$(function () {
    GetAllRoles();
    $("#user_role_datatables").DataTable({});
});


// Get all roles
function GetAllRoles() {
    $.ajax({
        url: 'Roles/GetRoles',
        type: 'get',
        dataType: 'json',
        success: function (response) {
            let roleTableData = '';

            if (!response || response.length === 0) {
                if ($.fn.DataTable.isDataTable('#user_role_datatables')) {
                    $('#user_role_datatables').DataTable().destroy();
                }

                $('#user_role_datatables').DataTable();
                return;
            }

            let counter = 1;
            $.each(response, function (index, item) {

                roleTableData += `
                    <tr>
                        <td>${counter++}</td>
                        <td>${item.name}</td>
                        <td>
                            <div class="d-grid gap-2 d-md-block">
                                 <button class="btn btn-sm btn-primary" onclick="Edit('${item.id}')">
                                    <span class="btn-label">
                                      <i class="fa fa-edit"></i>
                                    </span>
                                  </button>
                                  <button class="btn btn-sm btn-danger" onclick="Delete('${item.id}')">
                                    <span class="btn-label">
                                      <i class="fa fa-trash"></i>
                                    </span>
                                  </button>
                            </div>
                        </td>
                    </tr>`;
            });

            if ($.fn.DataTable.isDataTable('#user_role_datatables')) {
                $('#user_role_datatables').DataTable().destroy();
            }
            $('#tableRoles').html(roleTableData);
            $('#user_role_datatables').DataTable();
        },
        error: ajaxErrorHandler('Unable to read the data.')
    });
}

//<td>
//    <div class="btn-group dropdown">
//        <button class="btn btn-sm btn-black btn-border dropdown-toggle"
//            type="button"
//            data-bs-toggle="dropdown">
//            Options
//        </button>
//        <ul class="dropdown-menu" role="menu">
//            <li>
//                <a href="#" class="dropdown-item" type="button" onclick="Edit('${item.id}')">Edit</a>
//                <a class="dropdown-item" href="#" type="button" onclick="Delete('${item.id}')">Delete</a>
//            </li>
//        </ul>
//    </div>
//</td>

function ajaxErrorHandler(errorMessage = 'Unable to process the resquest.') {
    return function () {
        toastr.error(errorMessage);
    };
}

// Add New Roles
$('#btnAdd').on('click',function () {
    showModal('New Role', 'Save')
});

function Insert() {
    if (!Validate()) return;

    let roleName = $('#Role_Name').val();

    $.ajax({
        url: 'Roles/Create',
        data: { roleName: roleName },
        type: 'post',
        contentType: 'application/x-www-form-urlencoded',
        success: function (response) {
            if (response.success) {
                processResponse(response.success, response.message);
            }
            else {
                toastr.error(response.message || 'An error occured while saving the data.');
            }
            
        },
        error: ajaxErrorHandler('Unable to save data.')
    });
    
}

// Get Role for Edit
function Edit(id) {
    $.ajax({
        url: 'Roles/Edit',
        type: 'get',
        dataType: 'json',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                $('#Role_Name').val(response.role.name); // Access nested `role.name`
                $('#Role_Id').val(response.role.id);
                showModal('Update Role', 'Update');
            } else {
                toastr.error(response.message || 'Unable to read the data.');
            }
        },
        error: function () {
            toastr.error('Unable to read the data.');
        }
    });
}

// Update
function Update() {
    if (!Validate) return;

    const formData = new Object();
    formData.id = $('#Role_Id').val();
    formData.name = $('#Role_Name').val();

    $.ajax({
        url: 'Roles/Update',
        data: formData,
        type: 'post',
        success: function (response) {
            if (response.success) {
                processResponse(response.success, response.message);

            }
            else {
                toastr.error(response.message);
            }
        },
        error: ajaxErrorHandler('Unable to save data')
    });
}

// Delete 
function Delete(id) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel',
        reverseButton: true
    }).then((result) => {
        if (result.value) {
            $.ajax({
                url: `Roles/Delete?id=${id}`,
                type: 'post',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        GetAllRoles();
                    }
                    else {
                        toastr.error(response.message);
                    }
                },
                error: ajaxErrorHandler('Unable to delete the data.')
            });
        }
    });
}

function showModal(title, action) {
    $('#RoleModal').modal('show');
    $('#modalTitle').text(title);
    $('#Save').toggle(action === 'Save');
    $('#Update').toggle(action === 'Update');
}


function Validate() {
    let isValid = true;

    if (!$('#Role_Name').val().trim()) {
        showValidationError('#ValidationName', 'Please enter role name.');
        isValid = false;
    }
    else {
        $('#ValidationName').css('display', 'none');
    }

    return isValid;
}

function showValidationError(selector, message) {
    $(selector).text(message).show();
}

function processResponse(response, successMessage) {
    if (response) {
        toastr.success(successMessage);
        HideModal();
        GetAllRoles();
    }
    else {
        toastr.error('Operation failed');
    }
}

function HideModal() {
    $('#Role_Name').val('');
    $('#ValidationName').css('display', 'none');
    $('#RoleModal').modal('hide');
}