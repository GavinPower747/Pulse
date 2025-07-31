import {Controller, register} from '../framework/index.js'

class ErrorToastController extends Controller {
    connect() {
        this.context.addEventListener('htmx:responseError', this._onError.bind(this));
        this.context.addEventListener('htmx:error', this._onError.bind(this));
        this.closeButton.addEventListener('click', this._closeToast.bind(this));
    }

    _onError() {
        this._showToast();

        setTimeout(() => {
            this._closeToast();
        }, 5000);
    }

    _showToast() {
        this.context.removeAttribute('hidden');
    }

    _closeToast() {
        this.context.setAttribute('hidden', '');
    }
}

register('error-toast', ErrorToastController);