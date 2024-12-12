

$(function () {
    GetAllPriorityLevel();
    $("#priority_level_datatables").DataTable({});
})

// Get all prioritylevel
function GetAllPriorityLevel() {
    $.ajax({
        url: 'PriorityLevel/GetAllPriorityLevels',
        typr: 'get',
        dataType: 'json',
        success: function (response) {
            let plevelTableData = '';

            if (response.success) {
                let counter = 1;
                $.each(response.pLevels, function (index, item) {
                    plevelTableData += `
                        <tr>
                            <td>${counter}</td>
                            <td>
                                <span class="badge badge-${item.priorityLevelColor}" 
                                    style="font-size: 1.0em; padding: 0.2em 0.5;">
                                    ${item.priorityLevelName}
                                </span>
                            </td>
                            <td>
                                <div class="d-grid gap-2 d-md-block">
                                     <button class="btn btn-sm btn-primary" onclick="EditPLevel(${item.id})">
                                        <span class="btn-label">
                                          <i class="fa fa-edit"></i>
                                        </span>
                                      </button>
                                      <button class="btn btn-sm btn-danger" onclick="DeletePLevel(${item.id})">
                                        <span class="btn-label">
                                          <i class="fa fa-trash"></i>
                                        </span>
                                      </button>
                                </div>
                            </td>
                        </tr>
                    `;
                });

                // Destroy DataTable and Reinitialise
                if ($.fn.DataTable.isDataTable("#priority_level_datatables")) {
                    $('#priority_level_datatables').DataTable().destroy();
                }
                $('#plevelsTableBody').html(plevelTableData);
                $('#priority_level_datatables').DataTable();

            }
            else if (!response.plevels) {
                if ($.fn.DataTable.isDataTable("#priority_level_datatables")) {
                    $('#priority_level_datatables').DataTable().destroy();
                }
                $('#priority_level_datatables').DataTable();
            }
            else {
                toastr.error(response.message);
            }
        }

    })

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
//                <button class="dropdown-item"
//                    data-bs-toggle="modal"
//                    data-bs-target="#modal-form-edit-plevel">Edit</button>
//                <a class="dropdown-item" href="#" type="button"
//                    data-bs-toggle="modal"
//                    data-bs-target="#modal-notification-delete-plevel">Delete</a>
//            </li>
//        </ul>
//    </div>
//</td>

// Add Priority Levels
$('#btnAddPLevel').on('click', function () {
    showModalPLevel('New Priority Level', 'Save');
});

$('#SavePLevel').on('click', function (){
    if (ValidatePLevelInput()) {
        const formData = getPlevelInputData();

        $.ajax({
            url: 'PriorityLevel/Create',
            data: formData,
            type: 'post',
            success: function (response) {
                if (response.success) {
                    processResponsePLevel(true, response.message);
                }
                else {
                    toastr.error(response.message || 'An error occured while saving the data.');
                }
            },
            error: ajaxErrorHandler('Unable to save data.')
        });
    }
})
function EditPLevel(id) {

    $.ajax({
        url: `PriorityLevel/Edit?id=${id}`,
        type: 'get',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                populateFormDataPLevel(response.pLevel);
                showModalPLevel('Update Priority Level', 'Update');
            }
        },
        error: ajaxErrorHandler('Unable to retrieve department data.')

    })
}

function UpdatePLevel() {
    if (!ValidatePLevelInput()) return;

    const formData = getPlevelInputData($('#pLevelID').val());

    $.ajax({
        url: 'PriorityLevel/Update',
        data: formData,
        type: 'post',
        success: function (response) {
            if (response.success) {
                processResponsePLevel(true, response.message);
            }
            else {
                toastr.error(response.message);
            }
        },
        error: ajaxErrorHandler('Unable to save data.')

    });
}


function populateFormDataPLevel(data) {
    $('#pLevelID').val(data.id);
    $('#PriorityLevel_PriorityLevelName').val(data.priorityLevelName).val();

    $(`input[name="color"][value="${data.priorityLevelColor}"]`).prop('checked', true);

    $('#PriorityLevel_PriorityLevelDescription').val(data.priorityLevelDescription);

    if (data.isPriorityLevelActive == 1) {
        $('#PriorityLevel_IsPriorityLevelActive').prop('checked', true);
    }
    else {
        $('#PriorityLevel_IsPriorityLevelActive').prop('checked', false);
    }
}

function DeletePLevel(id) {

    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        type: 'warning',
        showCancelButton: true,
        confirmBUttonText: 'Yes, delete it!',
        cancelButtonText: 'Cancel',
        reverseButtons: true
    }).then((result) => {
        if (result.value) {
            $.ajax({
                url: `PriorityLevel/Delete?id=${id}`,
                type: 'post',
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        GetAllPriorityLevel();
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

function showModalPLevel(title, action) {
    $('#plevelModal').modal('show');
    $('#plevelModalTitle').text(title);
    // If the element is currently visible, it will be hidden; 
    // if it's hidden, it will be shown.
    $('#SavePLevel').toggle(action === 'Save'); 
    $('#UpdatePLevel').toggle(action === 'Update');
}

function getPlevelInputData(id = null) {
    const formData = {
        priorityLevelName: $('#PriorityLevel_PriorityLevelName').val(), // Get priority level name
        priorityLevelColor: $('input[name="color"]:checked').val(),    // Get selected color value
        priorityLevelDescription: $('#PriorityLevel_PriorityLevelDescription').val(), // Get description
        isPriorityLevelActive: $('#PriorityLevel_IsPriorityLevelActive').prop('checked') // Check if active
    };

    // Optionally include the ID if it's provided
    if (id) {
        formData.id = id;
    }

    return formData;
}

function processResponsePLevel(response, successMessage) {
    if (response) {
        toastr.success(successMessage);
        HideModalPLevel();
        GetAllPriorityLevel();
    }
    else {
        toastr.error('Operation failed');
    }

}
function HideModalPLevel() {
    ClearDataPLevel();
    $('#plevelModal').modal('hide');
}

function ClearDataPLevel() {
    $('#PriorityLevel_PriorityLevelName, #PriorityLevel_PriorityLevelDescription').val('');
    $('input[name="color"').prop('checked', false);
    $('#PriorityLevel_IsPriorityLevelActive').prop('checked', false);
    // Clear validation error messages
    $('#ValidationPLevelName, #ValidationPriorityLevelColor, #ValidationPriorityLevelDescription').text('');
}

function ValidatePLevelInput() {

    let isValid = true;
    const validationMessage = {
        pLevelName: 'Please enter priority level name.',
        pLevelColor: 'Please select one color.',
        pLevelDescription: 'Please input the priority level description.'
    };

    if (!$('#PriorityLevel_PriorityLevelName').val().trim()) {
        showValidationError('#ValidationPLevelName', validationMessage.pLevelName);
        isValid = false;
    }
    else {
        $('#ValidationPLevelName').css('display', 'none');
    }

    if (!$('input[name="color"]:checked').length) {
        showValidationError('#ValidationPriorityLevelColor', validationMessage.pLevelColor);
        isValid = false;
    }
    else {
        $('#ValidationPriorityLevelColor').css('display', 'none');
    }

    if (!$('#PriorityLevel_PriorityLevelDescription').val().trim()) {
        showValidationError('#ValidationPriorityLevelDescription', validationMessage.pLevelDescription);
        isValid = false;
    }
    else {
        $('#ValidationPriorityLevelDescription').css('display', 'none');
    }

    return isValid;

}

