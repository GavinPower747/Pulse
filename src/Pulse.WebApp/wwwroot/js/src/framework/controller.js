import { capitalize, camelCaseToKebabCase, kebabToCamelCase } from "utils/strings.js";

/**@typedef {import(../types/framework)} */

/**
 * Base class for controllers
 */
export class Controller {
    /** @type {HTMLElement} */
    context;
    /** @type {string} */
    _dataPrefix = 'data-js-';
    /** @type {Map<string, Component>} */
    _components;
    /** @type {MutationObserver} */
    attributeObserver;

    /**
     * @param {HTMLElement} context 
     * @returns {Controller}
     */
    constructor(context) {
        this.context = context;
        this._components = new Map();

        this._initializeComponents();

        this.attributeObserver = new MutationObserver(this._handleAttributeChange.bind(this));
        this.attributeObserver.observe(this.context, {
            attributes: true,
            subtree: true,
            attributeFilter: this._getObservedAttributes()
        });

        return new Proxy(this, {
            get: (target, prop) => {
                if (prop in target) {
                    return target[prop];
                } else if (typeof prop === 'string' && isDataProperty(prop, this.context, this._dataPrefix)) {
                    const attributeName = propertyToAttribute(prop, this._dataPrefix);
                    const rawValue = this.context.getAttribute(attributeName);
                    return parseDataAttributeValue(rawValue);
                }
                return undefined;
            },
            set: (target, prop, value) => {
                if (prop in target) {
                    target[prop] = value;
                } else if (typeof prop === 'string' && isDataProperty(prop, this.context, this._dataPrefix)) {
                    const attributeName = propertyToAttribute(prop, this._dataPrefix);
                    this.context.setAttribute(attributeName, value);
                }
                return true;
            }
        });
    }

    connect() {
        // Override this method in a subclass to handle when connected to the DOM    
    }

    disconnect() {
        // Override this method in a subclass to handle when disconnected from the DOM
        this.attributeObserver?.disconnect();
        // Clean up component observers
        for (const component of this._components.values()) {
            component._cleanup?.();
        }
    }

    /**
     * Dispatches a custom event with the given name and options.
     * @param {string} eventName - The name of the event to dispatch.
     * @param {Object} options - Options for the event dispatch.
     * @param {string} [options.prefix] - Prefix for the event name.
     * @param {*} [options.detail] - Event detail data.
     * @param {boolean} [options.bubbles=true] - Whether the event bubbles.
     * @param {boolean} [options.cancelable=true] - Whether the event is cancelable.
     * @param {EventTarget} [options.target] - Target to dispatch the event on.
     * @returns {CustomEvent} The dispatched event.
     */
    dispatch(eventName, options = {}) {
        const { prefix, detail, bubbles = true, cancelable = true, target = this.context } = options;
        const type = prefix ? `${prefix}-${eventName}` : eventName;
        const event = new CustomEvent(type, { detail, bubbles, cancelable });
        target.dispatchEvent(event);
        return event;
    }

    /**
     * Initializes all components found within the controller's context.
     * @private
     */
    _initializeComponents() {
        const componentElements = this.context.querySelectorAll('[data-component]');
        
        for (const element of componentElements) {
            const parentController = element.closest('[data-controller]');
            if (parentController && parentController !== this.context) {
                continue; // Skip components that are not direct children of this controller
            }

            const componentName = element.getAttribute('data-component');
            if (componentName) {
                const camelCaseName = kebabToCamelCase(componentName);
                const component = new Component(element, this._dataPrefix);
                this._components.set(camelCaseName, component);
                
                // Add component as property to controller
                Object.defineProperty(this, camelCaseName, {
                    value: component,
                    writable: false,
                    enumerable: true,
                    configurable: false
                });
            }
        }
    }

    /**
     * Gets all attributes that should be observed by the mutation observer.
     * @private
     * @returns {string[]} Array of attribute names to observe.
     */
    _getObservedAttributes() {
        const attributes = [];
        
        const controllerDataAttrs = Array.from(this.context.attributes)
            .filter(attr => attr.name.startsWith(this._dataPrefix))
            .map(attr => attr.name);
        attributes.push(...controllerDataAttrs);

        for (const component of this._components.values()) {
            const componentDataAttrs = Array.from(component.context.attributes)
                .filter(attr => attr.name.startsWith(this._dataPrefix))
                .map(attr => attr.name);
            attributes.push(...componentDataAttrs);
        }

        return [...new Set(attributes)];
    }

    /**
     * Handles attribute changes observed by the MutationObserver.
     * @private
     * @param {MutationRecord[]} mutations - The list of mutations that occurred.
     */
    _handleAttributeChange(mutations) {
        for (const mutation of mutations) {
            if (mutation.type === 'attributes' && mutation.attributeName?.startsWith(this._dataPrefix)) {
                const target = mutation.target;
                
                if (target === this.context) {
                    this._handleControllerAttributeChange(mutation);
                } else {
                    this._handleComponentAttributeChange(mutation, target);
                }
            }
        }
    }

    /**
     * Handles attribute changes on the controller's context element.
     * @private
     * @param {MutationRecord} mutation - The mutation record.
     */
    _handleControllerAttributeChange(mutation) {
        const prop = attributeToProperty(mutation.attributeName, this._dataPrefix);
        const rawValue = this.context.getAttribute(mutation.attributeName);
        const newValue = parseDataAttributeValue(rawValue);
        const oldValue = parseDataAttributeValue(mutation.oldValue);
        
        if (typeof this[`${prop}Changed`] === 'function') {
            this[`${prop}Changed`](newValue, oldValue);
        }
    }

    /**
     * Handles attribute changes on component elements.
     * @private
     * @param {MutationRecord} mutation - The mutation record.
     * @param {HTMLElement} target - The target element.
     */
    _handleComponentAttributeChange(mutation, target) {
        for (const [componentName, component] of this._components.entries()) {
            if (component.context === target) {
                const prop = attributeToProperty(mutation.attributeName, this._dataPrefix);
                const rawValue = target.getAttribute(mutation.attributeName);
                const newValue = parseDataAttributeValue(rawValue);
                const oldValue = parseDataAttributeValue(mutation.oldValue);
                
                if (typeof this[`${componentName}${capitalize(prop)}Changed`] === 'function') {
                    this[`${componentName}${capitalize(prop)}Changed`](newValue, oldValue);
                }

                break;
            }
        }
    } 
}

/**
 * A class representing a component within a controller.
 * Components act as sub-controllers that attach themselves to children of the controller's context.
 * @class Component
 * @property {HTMLElement} context - The DOM element that this component is attached to.
 */
class Component {
    constructor(context, dataPrefix) {
        this.context = context;
        this._dataPrefix = dataPrefix;

        return new Proxy(this, {
            get: (target, prop) => {
                if (prop in target) {
                    return target[prop];
                } else if (typeof prop === 'string' && isDataProperty(prop, this.context, this._dataPrefix)) {
                    const attributeName = propertyToAttribute(prop, this._dataPrefix);
                    const rawValue = this.context.getAttribute(attributeName);
                    return parseDataAttributeValue(rawValue);
                } else if (prop in this.context) {
                    let value = this.context[prop];
                    return (typeof value === 'function') 
                        ? value.bind(this.context) 
                        : value;
                }

                return undefined;
            },
            set: (target, prop, value) => {
                if (prop in target) {
                    target[prop] = value;
                } else if (typeof prop === 'string' && isDataProperty(prop, this.context, this._dataPrefix)) {
                    const attributeName = propertyToAttribute(prop, this._dataPrefix);
                    this.context.setAttribute(attributeName, value);
                } else if (prop in this.context) {
                    this.context[prop] = value;
                }

                return true;
            }
        });
    }

    /**
     * Adds an event listener to the component's context element.
     * @param {string} eventName - The event name.
     * @param {Function} callback - The event callback.
     * @param {Object|boolean} [options] - Event listener options.
     */
    addEventListener(eventName, callback, options) {
        this.context.addEventListener(eventName, callback, options);
    }

    /**
     * Removes an event listener from the component's context element.
     * @param {string} eventName - The event name.
     * @param {Function} callback - The event callback.
     * @param {Object|boolean} [options] - Event listener options.
     */
    removeEventListener(eventName, callback, options) {
        this.context.removeEventListener(eventName, callback, options);
    }
}

/**
 * Checks if a property name should be treated as a data property.
 * @param {string} prop - The property name.
 * @returns {boolean} True if it's a data property.
 */
function isDataProperty(prop, node, prefix) {
    const attributeName = propertyToAttribute(prop, prefix);
    return node.hasAttribute(attributeName);
}

/**
 * Converts a property name to its corresponding attribute name.
 * @param {string} prop - The property name.
 * @returns {string} The attribute name.
 */
function propertyToAttribute(prop, prefix) {
    return `${prefix}${camelCaseToKebabCase(prop)}`;
}

/**
 * Converts an attribute name to its corresponding property name.
 * @param {string} attributeName - The attribute name.
 * @returns {string} The property name.
 */
function attributeToProperty(attributeName, prefix) {
    return attributeName
        .replace(new RegExp(`^${prefix}`), '')
        .replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
}

/**
 * Parses a data attribute value based on its content.
 * Supports: numbers, booleans, arrays of numbers, arrays of strings.
 * Everything else remains as strings.
 * @param {string} value - The raw attribute value.
 * @returns {*} The parsed value.
 */
function parseDataAttributeValue(value) {
    if (value === null || value === undefined) {
        return value;
    }

    const trimmed = value.trim();
    
    if (trimmed === '') {
        return '';
    }

    if (trimmed === 'true') {
        return true;
    }

    if (trimmed === 'false') {
        return false;
    }

    if (trimmed.startsWith('[') && trimmed.endsWith(']')) {
        try {
            const parsed = JSON.parse(trimmed);
            if (Array.isArray(parsed)) {
                if (parsed.every(item => typeof item === 'number' && !isNaN(item))) {
                    return parsed;
                }

                if (parsed.every(item => typeof item === 'string')) {
                    return parsed;
                }

                return trimmed;
            }
        } catch (e) {
            // If JSON parsing fails, return as string
            return trimmed;
        }
    }

    const num = Number.parseFloat(trimmed);
    if (Number.isFinite(num)) {
        return num;
    }

    return trimmed;
}
