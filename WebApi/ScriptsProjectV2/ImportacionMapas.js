
function ConfirmarEliminacion(idVersion) {
    var result = confirm("Está seguro de eliminar la versión: " + idVersion);
    if (result) {
        var paramJson = idVersion;
        callAction("/importacion/EliminarMapas/", { paramJson: paramJson }, function (res) {
            location.reload();
        });

    }
}


function BtnImportar() {
    event.preventDefault();
    var formdata = new FormData(); //FormData object
    var fileInputBase = document.getElementById('formFileBase');
    if (fileInputBase.files.length != 1) {
        alert("Indique un fichero base");
        return;
    }
    var fileInputCatastral = document.getElementById('formFileCatastral');
    if (fileInputCatastral.files.length != 1) {
        alert("Indique un fichero catastral");
        return;
    }
    formdata.append(fileInputBase.files[0].name, fileInputBase.files[0]);
    formdata.append(fileInputCatastral.files[0].name, fileInputCatastral.files[0]);
    var idVersion = document.getElementById('formIdVersion').value;
    if (idVersion == '') {
        alert("Indique versíón");
        retur;
    }
    formdata.append("idVersion", idVersion);
    //Creating an XMLHttpRequest and sending
    var xhr = new XMLHttpRequest();
    xhr.open('POST', urlPost);
    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {
            if (xhr.response == 'OK') {
                location.reload();
            }
            else {
                alert(xhr.response);
                location.reload();
            }
        }
    };
    xhr.send(formdata);

    alert("La carga de los mapas puede ser lenta. La página se regargará automáticamente al finilizar la carga. Si lo desea puede recargar la página para seguir el proceso");
    //location.reload();
}


function sleep(delay) {
    var start = new Date().getTime();
    while (new Date().getTime() < start + delay);
}

function callAction(action, parametros, funcSuccess) {
    $.ajax({
        type: "POST",
        url: action,
        data: parametros,
        datatype: "json",
        async: true,
        contentType: 'application/x-www-form-urlencoded',
        success: function (respuesta) {
            try {
                if (respuesta.substring(0, 2) != "OK")
                    alert({ type: 'error', title: 'Oops...', text: respuesta })
                else
                    funcSuccess();

            } catch (err) {
                alert('Error en llamada al controlador');
            }
        },
        error: function () {
            alert('Error en llamada al controlador');
        }
    })
}

function getFormDataArray($form) {
    var unindexed_array = $form.serializeArray();
    var indexed_array = {};

    $.map(unindexed_array, function (n, i) {
        indexed_array[n['name']] = n['value'];
    });
    return indexed_array;
}

