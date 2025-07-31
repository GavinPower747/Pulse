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
                } else if (typeof prop === 'string' && this._isDataProperty(prop)) {
                    const attributeName = this._propertyToAttribute(prop);
                    return this.context.getAttribute(attributeName);
                }
                return undefined;
            },
            set: (target, prop, value) => {
                if (prop in target) {
                    target[prop] = value;
                } else if (typeof prop === 'string' && this._isDataProperty(prop)) {
                    const attributeName = this._propertyToAttribute(prop);
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
            const componentName = element.getAttribute('data-component');
            if (componentName) {
                const camelCaseName = this._kebabToCamelCase(componentName);
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
        const prop = this._attributeToProperty(mutation.attributeName);
        const newValue = this.context.getAttribute(mutation.attributeName);
        
        if (typeof this[`${prop}Changed`] === 'function') {
            this[`${prop}Changed`](newValue, mutation.oldValue);
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
                const prop = this._attributeToProperty(mutation.attributeName);
                const newValue = target.getAttribute(mutation.attributeName);
                
                if (typeof this[`${componentName}${this._capitalize(prop)}Changed`] === 'function') {
                    this[`${componentName}${this._capitalize(prop)}Changed`](newValue, mutation.oldValue);
                }

                break;
            }
        }
    }

    /**
     * Checks if a property name should be treated as a data property.
     * @private
     * @param {string} prop - The property name.
     * @returns {boolean} True if it's a data property.
     */
    _isDataProperty(prop) {
        const attributeName = this._propertyToAttribute(prop);
        return this.context.hasAttribute(attributeName);
    }

    /**
     * Converts a property name to its corresponding attribute name.
     * @private
     * @param {string} prop - The property name.
     * @returns {string} The attribute name.
     */
    _propertyToAttribute(prop) {
        const kebabCase = prop.replace(/[A-Z]/g, letter => `-${letter.toLowerCase()}`);
        return `${this._dataPrefix}${kebabCase}`;
    }

    /**
     * Converts an attribute name to its corresponding property name.
     * @private
     * @param {string} attributeName - The attribute name.
     * @returns {string} The property name.
     */
    _attributeToProperty(attributeName) {
        return attributeName
            .replace(new RegExp(`^${this._dataPrefix}`), '')
            .replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
    }

    /**
     * Converts kebab-case to camelCase.
     * @private
     * @param {string} str - The kebab-case string.
     * @returns {string} The camelCase string.
     */
    _kebabToCamelCase(str) {
        return str.replace(/-([a-z])/g, (_, letter) => letter.toUpperCase());
    }

    /**
     * Capitalizes the first letter of a string.
     * @private
     * @param {string} str - The string to capitalize.
     * @returns {string} The capitalized string.
     */
    _capitalize(str) {
        return str.charAt(0).toUpperCase() + str.slice(1);
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
                } else if (typeof prop === 'string' && this._isDataProperty(prop)) {
                    const attributeName = this._propertyToAttribute(prop);
                    return this.context.getAttribute(attributeName);
                }
                return undefined;
            },
            set: (target, prop, value) => {
                if (prop in target) {
                    target[prop] = value;
                } else if (typeof prop === 'string' && this._isDataProperty(prop)) {
                    const attributeName = this._propertyToAttribute(prop);
                    this.context.setAttribute(attributeName, value);
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

    /**
     * Checks if a property name should be treated as a data property.
     * @private
     * @param {string} prop - The property name.
     * @returns {boolean} True if it's a data property.
     */
    _isDataProperty(prop) {
        // Check if property would map to a data-js- attribute
        const attributeName = this._propertyToAttribute(prop);
        return this.context.hasAttribute(attributeName);
    }

    /**
     * Converts a property name to its corresponding attribute name.
     * @private
     * @param {string} prop - The property name.
     * @returns {string} The attribute name.
     */
    _propertyToAttribute(prop) {
        const kebabCase = prop.replace(/[A-Z]/g, letter => `-${letter.toLowerCase()}`);
        return `${this._dataPrefix}${kebabCase}`;
    }
}