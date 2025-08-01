import {Controller} from 'framework'

export default class ErrorToastController extends Controller {
    connect() {
        document.body.addEventListener('htmx:responseError', this._onError.bind(this));
        document.body.addEventListener('htmx:error', this._onError.bind(this));
        document.body.addEventListener('htmx:sendError', this._onError.bind(this));
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
