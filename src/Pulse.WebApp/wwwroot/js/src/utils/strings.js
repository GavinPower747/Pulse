/**
 * Converts kebab-case to camelCase.
 * @param {string} str - The kebab-case string.
 * @returns {string} The camelCase string.
 */
export function kebabToCamelCase(str) {
    return str.replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
}

/**
 * Converts kebab-case to PascalCase.
 * @param {string} str - The kebab-case string.
 * @returns {string} The PascalCase string.
 */
export function kebabToPascalCase(str) {
    return str.split('-').map(word => word.charAt(0).toUpperCase() + word.slice(1)).join('');
}

/**
 * Converts camelCase to kebab-case.
 * @param {string} str - The camelCase string.
 * @returns {string} The kebab-case string.
 */
export function camelCaseToKebabCase(str) {
    return str.replace(/[A-Z]/g, letter => `-${letter.toLowerCase()}`);
}

/**
 * Capitalizes the first letter of a string.
 * @param {string} str - The string to capitalize.
 * @returns {string} The capitalized string.
 */
export function capitalize(str) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}
