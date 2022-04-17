
$(document).ready(() => {
    if ($('#containerTablaDescargar').is(':visible')){
        obtenerArchivos();
    }
})

function subirArchivos() {
    var totalFiles = document.getElementById('inputSubirArchivos').files.length;
    var formData = new FormData();

    var esValido = true;
    if (totalFiles == 0) {
        esValido = false;
    }

    if (esValido == true) {

        for (var i = 0; i < totalFiles; i++) {
            formData.append("archivos", document.getElementById('inputSubirArchivos').files[i]);
        }

        $.ajax({
            type: "POST",
            url: "/Home/InsertarArchivos",
            data: formData,
            contentType: false,
            processData: false,
            cache: false,
            beforeSend: function () {
                $('#loaderArchivos').html(`<p>SUBIENDO ARCHIVOS...</p>`);
            },
            success: function (response) {
                var respuesta = response;
                console.log(respuesta);

                $('#loaderArchivos').html(`<p>${respuesta.Mensaje_Respuesta}</p>`);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR);
                console.log(textStatus);
                console.log(errorThrown);
                alert("Ocurrió un error al verificar los CFDI(s): " + jqXHR);
            }
        });

    }

}


function obtenerArchivos() {

    $.ajax({
        type: "GET",
        url: "/Home/ConsultarArchivos",
        beforeSend: function () {
            $('#bodyTablaDescargar').html(`<tr><td colspan="5">CARGANDO ARCHIVOS...</td></tr>`);
        },
        success: function (response) {
            var respuesta = response;
            console.log(respuesta);

            if (respuesta.Codigo == 1) {
                var codigoHtml = "";
                for (var i = 0; i < respuesta.Archivos.length; i++) {
                    var filaActual = respuesta.Archivos[i];

                    codigoHtml += `
                <tr>
                    <td>${i + 1}</td>
                    <td>${filaActual.Nombre_Archivo}</td>
                    <td>${filaActual.Extension}</td>
                    <td>${filaActual.Fecha_Entrada}</td>
                    <td>${filaActual.Tamanio}MB</td>
                    <td><button class="btn btn-primary" onclick="descargarArchivo(${filaActual.Id}, '${filaActual.Formato}', '${filaActual.Nombre_Archivo + filaActual.Extension}')">Descargar</button></td>
                </tr>
                `;
                }
                $('#bodyTablaDescargar').html(codigoHtml);
            } else {
                alert("No ha retornado de forma correcta");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR);
            console.log(textStatus);
            console.log(errorThrown);
            alert("Ocurrió un error al verificar los CFDI(s): " + jqXHR);
        }
    });
}

function descargarArchivo(id, formato, nombreArchivo) {
    $.ajax({
        type: "POST",
        url: "/Home/ObtenerArchivo",
        data: { id },
        success: function (response) {
            var respuesta = response;

            const blob = base64ToBlob(respuesta.Mensaje_Respuesta, formato);
            guardarArchivo(blob, nombreArchivo);

        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR);
            console.log(textStatus);
            console.log(errorThrown);
            alert("Ocurrió un error al verificar los CFDI(s): " + jqXHR);
        }
    });
}

function base64ToBlob(base64, type = "application/octet-stream") {
    const binStr = atob(base64);
    const len = binStr.length;
    const arr = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
        arr[i] = binStr.charCodeAt(i);
    }
    return new Blob([arr], { type: type });
}


function guardarArchivo(blob, filename) {
    if (window.navigator.msSaveOrOpenBlob) {
        window.navigator.msSaveOrOpenBlob(blob, filename);
    } else {
        const a = document.createElement('a');
        document.body.appendChild(a);
        const url = window.URL.createObjectURL(blob);
        a.href = url;
        a.download = filename;
        a.click();
        setTimeout(() => {
            window.URL.revokeObjectURL(url);
            document.body.removeChild(a);
        }, 0)
    }
}