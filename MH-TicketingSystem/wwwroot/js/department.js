$(function () {
    GetAllDepartments();
    $("#department_datatable").DataTable({});
});


// Get all departments ---------------------------------------------------------------------------------------------------------------
function GetAllDepartments() {
    $.ajax({
        url: 'Department/GetAllDepartments',
        type: 'get',
        dataType: 'json',
        success: function (response) {
            let departmentTableData = '';

            if (!response || response.length === 0) {
                if ($.fn.DataTable.isDataTable("#department_datatable")) {
                    $('#department_datatable').DataTable().destroy();
                }
                $("#department_datatable").DataTable();
                return;
            }

            let counter = 1;

            $.each(response, function (index, item) {
                
                departmentTableData += `
                    <tr>
                        <td>${counter++}</td>
                        <td>${item.departmentCode}</td>
                        <td>${item.departmentName}</td>
                        <td>
                            <div class="d-grid gap-2 d-md-block">
                                 <button class="btn btn-sm btn-primary" onclick="Edit(${item.id})">
                                    <span class="btn-label">
                                      <i class="fa fa-edit"></i>
                                    </span>
                                  </button>
                                  <button class="btn btn-sm btn-danger" onclick="Delete(${item.id})">
                                    <span class="btn-label">
                                      <i class="fa fa-trash"></i>
                                    </span>
                                  </button>
                            </div>
                        </td>
                        
                    </tr>
                `;
            });

            if ($.fn.DataTable.isDataTable("#department_datatable")) {
                $('#department_datatable').DataTable().destroy();
            }
            $('#departmentTable').html(departmentTableData);
            $("#department_datatable").DataTable();
        },
        error: ajaxErrorHandler('Unable to read the data.')
    });
}

//<td>
//    <div class="btn-group dropdown">
//        <button class="btn btn-sm btn-black btn-border dropdown-toggle" type="button" data-bs-toggle="dropdown">
//            Options
//        </button>
//        <ul class="dropdown-menu" role="menu">
//            <li>
//                <a class="dropdown-item" type="button" onclick="Edit(${item.id})">Edit</a>
//                <a class="dropdown-item" type="button" onclick="Delete(${item.id})">Delete</a>
//            </li>
//        </ul>
//    </div>
//</td>

function ajaxErrorHandler(errorMessage = 'Unable to process the request.') {
    return function () {
        toastr.error(errorMessage);
    };
}

// Add Department ---------------------------------------------------------------------------------------------------------------
$('#btnAdd').on('click', function () {
    showModal('New Department', 'Save');
});


function Insert() {
    if (!Validate()) return;

    const formData = gatherFormData();

    $.ajax({
        url: '/Department/Create',
        data: formData,
        type: 'post',
        success: function (response) {
            if (response.success) {
                processResponse(true, 'Data saved successfully.');
            } else {
                // Use toastr for error message
                toastr.error(response.message || 'An error occurred while saving the data.');
            }
        },
        error: ajaxErrorHandler('Unable to save data.')
    });
}


// Get Data to Edit ---------------------------------------------------------------------------------------------------------------
function Edit(id) {
    $.ajax({
        url: `/Department/Edit?id=${id}`,
        type: 'get',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                populateFormData(response.department);
                showModal('Update Department', 'Update');
                return;
            }
            toastr.error(response.message || 'Unable to read the data.');
        },
        error: ajaxErrorHandler('Unable to retrieve department data.')
    });
}

// Update Data ---------------------------------------------------------------------------------------------------------------
function Update() {
    if (!Validate()) return;

    const formData = gatherFormData($('#Id').val());

    $.ajax({
        url: '/Department/Update',
        data: formData,
        type: 'post',
        success: function (response) {
            if (response.success) {
                processResponse(true, response.message);
            } else {
                toastr.error(response.message); // Show the error message
            }
        },
        error: ajaxErrorHandler('Unable to save data.')
    });
}

// Delete Data ---------------------------------------------------------------------------------------------------------------
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
                url: `/Department/Delete?id=${id}`,
                type: 'post',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message); // Show success message
                        GetAllDepartments(); // Refresh the department list
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


// ---------------------------------------------------------------------------------------------------------------
function processResponse(response, successMessage) {
    if (response) {
        toastr.success(successMessage);
        HideModal();
        GetAllDepartments();
    } else {
        toastr.error('Operation failed.');
    }
}

// ---------------------------------------------------------------------------------------------------------------
function showModal(title, action) {
    $('#DepartmentModal').modal('show');
    $('#modalTitle').text(title);
    $('#Save').toggle(action === 'Save');
    $('#Update').toggle(action === 'Update');
}

// ---------------------------------------------------------------------------------------------------------------
function HideModal() {
    ClearData();
    $('#DepartmentModal').modal('hide');
}

// ---------------------------------------------------------------------------------------------------------------
function ClearData() {
    $('#DepartmentCode, #DepartmentName, #roleSelect').val('');
    $('#IsDepartmentActive').prop('checked', false);
    $('.validation-message').hide();
    $('#Save').show();
    $('#Update').hide();
    $('#ValidationDepartmentCode').css('display', 'none');
    $('#ValidationDepartmentName').css('display', 'none');
    $('#ValidationRoleId').css('display', 'none');
}

// ---------------------------------------------------------------------------------------------------------------
function Validate() {
    let isValid = true;
    const validationMessages = {
        departmentCode: 'Please enter department code.',
        departmentName: 'Please enter department name.',
        roleId: 'Please select privileges.'
    };

    if (!$('#DepartmentCode').val().trim()) {
        showValidationError('#ValidationDepartmentCode', validationMessages.departmentCode);
        isValid = false;
    }
    else {
        $('#ValidationDepartmentCode').css('display', 'none');
    }

    if (!$('#DepartmentName').val().trim()) {
        showValidationError('#ValidationDepartmentName', validationMessages.departmentName);
        isValid = false;
    }
    else {
        $('#ValidationDepartmentName').css('display', 'none');
    }

    if (!$('#roleSelect').val()) {
        showValidationError('#ValidationRoleId', validationMessages.roleId);
        isValid = false;
    }
    else {
        $('#ValidationRoleId').css('display', 'none');
    }

    return isValid;
}

// ---------------------------------------------------------------------------------------------------------------
function showValidationError(selector, message) {
    $(selector).text(message).show();
}

// ---------------------------------------------------------------------------------------------------------------

function gatherFormData(id = null) {
    const formData = {
        departmentCode: $('#DepartmentCode').val(),
        departmentName: $('#DepartmentName').val(),
        roleId: $('#roleSelect').val(),
        // isDepartmentActive: $('#IsDepartmentActive').prop('checked') ? 1 : 0 
        isDepartmentActive: $('#IsDepartmentActive').prop('checked')
    };

    // Only include the id if it's provided (i.e., it's not null)
    if (id !== null) {
        formData.id = id;
    }

    return formData;
}

// ------------------------------------------------------------------------------------------------------------------

function populateFormData(data) {
    $('#Id').val(data.id);
    $('#DepartmentCode').val(data.departmentCode);
    $('#DepartmentName').val(data.departmentName);
    $('#roleSelect').val(data.roleId);

    if (data.isDepartmentActive == 1) {
        $('#IsDepartmentActive').prop('checked', true);
    }
    else {
        $('#IsDepartmentActive').prop('checked', false);
    }

}
