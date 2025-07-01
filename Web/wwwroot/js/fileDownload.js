/**
 * Utilidades para descarga de archivos
 * Este archivo contiene funciones para manejar la descarga de archivos desde Blazor
 */

/**
 * Descarga un archivo desde datos en base64
 * @param {string} fileName - Nombre del archivo a descargar
 * @param {string} base64Data - Datos del archivo en formato base64
 * @param {string} mimeType - Tipo MIME del archivo (opcional)
 */
function downloadFile(fileName, base64Data, mimeType = 'application/octet-stream') {
    try {
        // Validar parámetros
        if (!fileName || !base64Data) {
            throw new Error('El nombre del archivo y los datos son requeridos');
        }

        // Convert base64 to blob
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: mimeType });

        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.style.display = 'none';
        
        // Trigger download
        document.body.appendChild(link);
        link.click();
        
        // Cleanup
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
        
        console.log(`Archivo "${fileName}" descargado exitosamente`);
    } catch (error) {
        console.error('Error al descargar archivo:', error);
        alert('Error al descargar el archivo. Por favor, intente nuevamente.');
    }
}

/**
 * Descarga un archivo Excel desde datos en base64
 * @param {string} fileName - Nombre del archivo Excel a descargar
 * @param {string} base64Data - Datos del archivo Excel en formato base64
 */
function downloadExcelFile(fileName, base64Data) {
    const excelMimeType = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';
    downloadFile(fileName, base64Data, excelMimeType);
}

/**
 * Descarga un archivo PDF desde datos en base64
 * @param {string} fileName - Nombre del archivo PDF a descargar
 * @param {string} base64Data - Datos del archivo PDF en formato base64
 */
function downloadPdfFile(fileName, base64Data) {
    const pdfMimeType = 'application/pdf';
    downloadFile(fileName, base64Data, pdfMimeType);
}

/**
 * Descarga un archivo CSV desde datos en base64
 * @param {string} fileName - Nombre del archivo CSV a descargar
 * @param {string} base64Data - Datos del archivo CSV en formato base64
 */
function downloadCsvFile(fileName, base64Data) {
    const csvMimeType = 'text/csv';
    downloadFile(fileName, base64Data, csvMimeType);
}