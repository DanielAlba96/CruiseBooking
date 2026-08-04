let stripe;
let elements;
let paymentElement;

// Devuelve null si el Payment Element se montó bien, o el mensaje de error si falló.
// Stripe reporta los fallos de carga del Element por el evento "loaderror", no lanzando,
// asi que sin esta promesa el formulario quedaria en blanco sin ningun error en .NET.
export function initialize(publishableKey, clientSecret) {
    if (typeof Stripe === "undefined") {
        return Promise.resolve("No se ha podido cargar Stripe.js.");
    }

    // Si se ocultó y se volvió a mostrar el formulario, el Element anterior ya no
    // tiene su div en el DOM: hay que destruirlo antes de montar el nuevo.
    destroy();

    stripe = Stripe(publishableKey);

    elements = stripe.elements({
        clientSecret
    });

    paymentElement = elements.create("payment");

    return new Promise(resolve => {
        paymentElement.on("ready", () => resolve(null));
        paymentElement.on("loaderror", event => {
            console.error("[stripe] loaderror", event.error);
            resolve(event.error?.message ?? "No se pudo cargar el formulario de pago.");
        });
        paymentElement.mount("#payment-element");
    });
}

export function destroy() {
    if (paymentElement) {
        paymentElement.destroy();
        paymentElement = null;
    }
}

export async function confirmSetup() {
    const result = await stripe.confirmSetup({
        elements,
        redirect: "if_required"
    });

    return {
        paymentMethodId: result.setupIntent?.payment_method ?? null,
        error: result.error?.message ?? null
    };
}