/**
 * Utilidades para manejo de tooltips de Bootstrap
 * Este archivo contiene funciones específicas para inicializar y manejar tooltips
 */

/**
 * Inicializa todos los tooltips de Bootstrap en la página
 * @returns {Array} Lista de instancias de tooltip creadas
 */
function tooltipManager() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));
    return tooltipList;
}

/**
 * Destruye todos los tooltips activos en la página
 */
function destroyAllTooltips() {
    const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltips.forEach(tooltip => {
        const bsTooltip = bootstrap.Tooltip.getInstance(tooltip);
        if (bsTooltip) {
            bsTooltip.dispose();
        }
    });
}

/**
 * Actualiza los tooltips después de cambios en el DOM
 */
function refreshTooltips() {
    destroyAllTooltips();
    return tooltipManager();
}
