import { domReady } from "../dom/events.js";
import { Controller } from "./controller.js";

/**@type {Map<string, Controller>} */
let controllers = new Map();
/**@type {Map<string, new (...args: any[]) => Controller>} */
let registrations = new Map();
/**@type {string} */
let attribute = "data-controller";
/**@type {string} */
let selector = `[${attribute}]`;

const observer = new MutationObserver(handleMutations);
observer.observe(document.body, {
    childList: true,
    subtree: true,
});

/**
 * Attaches controllers to existing DOM nodes that match the selector.
 * @returns {Promise<void>}
 */
async function attachExistingControllers(selector) {
    await domReady();

    const nodes = document.querySelectorAll(selector);
    for (const node of nodes) {
        connectController(node);
    }
}

/**
 * Handles mutations observed by the MutationObserver.
 * @param {MutationRecord[]} mutations - The list of mutations observed.
 * @returns {void}
 */
function handleMutations(mutations) {
    for (const mutation of mutations) {
        for (const node of mutation.addedNodes) {
            if (node.nodeType === Node.ELEMENT_NODE && node.matches && node.matches(selector)) {
                connectController(node);
            }
        }

        for (const node of mutation.removedNodes) {
            if (node.nodeType === Node.ELEMENT_NODE && node.matches && node.matches(selector)) {
                disconnectController(node);
            }
        }
    }
}

/**
 * Registers a controller for the given DOM node.
 * @param {HTMLElement} node - The DOM node to register the controller for.
 * @returns {void}
 */
function connectController(node) {
    const controllerName = node.getAttribute(attribute);
    if (!controllerName) return;

    if (!controllers.has(controllerName)) {
        const ControllerClass = registrations.get(controllerName);

        if (!ControllerClass) {
            console.warn(`Controller class "${controllerName}" not found. Make sure it's registered before use.`);
            return;
        }

        const controllerInstance = new ControllerClass(node);
        controllers.set(controllerName, controllerInstance);
        controllerInstance.connect();
    }
}

/**
 * Unregisters a controller for the given DOM node.
 * @param {HTMLElement} node - The DOM node to unregister the controller for.
 * @return {void}
 */
function disconnectController(node) {
    const controllerName = node.getAttribute(attribute);
    if (!controllerName) return;

    const controllerInstance = controllers.get(controllerName);
    if (controllerInstance) {
        controllerInstance.disconnect();
        controllers.delete(controllerName);
    }
}

/**
 * Registers a controller with a name, allowing it to be attached to DOM elements.
 * 
 * @param {string} name 
 * @param {class} controllerClass 
 * @throws {Error} If the controller class does not extend the Controller base class.
 * @returns {void}
 */
function register(name, controllerClass) {
    if (!(controllerClass.prototype instanceof Controller)) {
        throw new Error("Controller class must extend the Controller base class.");
    }

    registrations.set(name, controllerClass);

    let selector = `[${attribute}="${name}"]`;
    attachExistingControllers(selector);
}

export { register };