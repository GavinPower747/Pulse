import { domReady } from "utils/dom.js";
import { kebabToPascalCase } from "utils/strings.js";
import { Controller } from "framework";

/**@type {WeakMap<HTMLElement, Controller>} */
let controllers = new WeakMap();
/**@type {Map<string, new (...args: any[]) => Controller>} */
let registrations = new Map();
/**@type {string} */
let attribute = "data-controller";
/**@type {string} */
let selector = `[${attribute}]`;

(async () => {
    await attachExistingControllers(selector);
})();

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
        await connectController(node);
    }
}

/**
 * Handles mutations observed by the MutationObserver.
 * @param {MutationRecord[]} mutations - The list of mutations observed.
 * @returns {void}
 */
async function handleMutations(mutations) {
    for (const mutation of mutations) {
        for (const node of mutation.addedNodes) {
            if(!(node instanceof Element)) continue;

            if (node.matches && node.matches(selector)) {
                await connectController(node);
            }

            await Promise.all(
                Array.from(node.querySelectorAll(selector)).map(element => connectController(element))
            );
        }

        for (const node of mutation.removedNodes) {
            if(!(node instanceof Element)) continue;

            if (node.matches && node.matches(selector)) {
                disconnectController(node);
            }

            node.querySelectorAll(selector).forEach(element => {
                disconnectController(element);
            });
        }
    }
}

/**
 * Registers a controller for the given DOM node.
 * @private
 * @param {HTMLElement} node - The DOM node to register the controller for.
 * @returns {Promise<void>}
 */
async function connectController(node) {
    const controllerName = node.getAttribute(attribute);
    if (!controllerName) return;

    // Check if this specific node already has a controller
    if (controllers.has(node)) return;

    let ControllerClass = registrations.get(controllerName);

    if (!ControllerClass) {
        try {
            const expectedClassName = `${kebabToPascalCase(controllerName)}Controller`;
            const path = `controllers/${controllerName}-controller.js`;

            const module = await import(path);
            ControllerClass = module[expectedClassName] || module.default;

            if (ControllerClass && typeof ControllerClass === 'function' && ControllerClass.prototype instanceof Controller) {
                registrations.set(controllerName, ControllerClass);
            } else {
                ControllerClass = null;
            }
        } catch (error) {
            console.error(`Error loading controller "${controllerName}":`, error);
            return;
        }
    }

    if (!ControllerClass) {
        console.warn(`Controller "${controllerName}" not found or does not extend Controller base class.`);
        return;
    }

    const controllerInstance = new ControllerClass(node);
    controllers.set(node, controllerInstance);
    controllerInstance.connect();
}

/**
 * Unregisters a controller for the given DOM node.
 * @param {HTMLElement} node - The DOM node to unregister the controller for.
 * @return {void}
 */
function disconnectController(node) {
    const controllerInstance = controllers.get(node);
    if (controllerInstance) {
        controllerInstance.disconnect();
        controllers.delete(node);
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