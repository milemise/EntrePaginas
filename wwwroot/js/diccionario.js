(() => {
    const libro = document.getElementById('diccionarioLibro');
    if (!libro) return;

    const aperturas = Array.from(libro.querySelectorAll('.diccionario-apertura'));
    const anterior = document.getElementById('paginaAnterior');
    const siguiente = document.getElementById('paginaSiguiente');
    const estado = document.getElementById('estadoPaginas');
    if (!aperturas.length || !anterior || !siguiente || !estado) return;

    let indiceActual = Number.parseInt(libro.dataset.spreadInicial || '0', 10);
    let bloqueado = false;
    const duracionAnimacion = 420;

    const actualizarControles = () => {
        anterior.disabled = indiceActual <= 0 || bloqueado;
        siguiente.disabled = indiceActual >= aperturas.length - 1 || bloqueado;
        const paginaInicial = indiceActual * 2 + 1;
        const paginaFinal = paginaInicial + 1;
        estado.textContent = `Páginas ${paginaInicial}–${paginaFinal} de ${aperturas.length * 2}`;
    };

    const cambiarApertura = (nuevoIndice, direccion) => {
        if (bloqueado || nuevoIndice < 0 || nuevoIndice >= aperturas.length || nuevoIndice === indiceActual) return;
        bloqueado = true;
        const actual = aperturas[indiceActual];
        const destino = aperturas[nuevoIndice];
        const claseSalida = direccion === 'adelante' ? 'sale-adelante' : 'sale-atras';
        const claseEntrada = direccion === 'adelante' ? 'entra-adelante' : 'entra-atras';

        actual.classList.add(claseSalida);
        destino.classList.add('is-active', claseEntrada);
        destino.inert = false;
        destino.setAttribute('aria-hidden', 'false');

        window.setTimeout(() => {
            actual.classList.remove('is-active', claseSalida);
            actual.inert = true;
            actual.setAttribute('aria-hidden', 'true');
            destino.classList.remove(claseEntrada);
            indiceActual = nuevoIndice;
            bloqueado = false;
            actualizarControles();
        }, duracionAnimacion);
    };

    anterior.addEventListener('click', () => cambiarApertura(indiceActual - 1, 'atras'));
    siguiente.addEventListener('click', () => cambiarApertura(indiceActual + 1, 'adelante'));

    document.addEventListener('keydown', (evento) => {
        if (evento.altKey || evento.ctrlKey || evento.metaKey) return;
        const elemento = evento.target;
        if (elemento instanceof HTMLElement && (elemento.matches('input, textarea, select, button, a') || elemento.isContentEditable)) return;
        if (evento.key === 'ArrowRight') {
            evento.preventDefault();
            cambiarApertura(indiceActual + 1, 'adelante');
        } else if (evento.key === 'ArrowLeft') {
            evento.preventDefault();
            cambiarApertura(indiceActual - 1, 'atras');
        }
    });

    let inicioToque = null;
    libro.addEventListener('touchstart', (evento) => {
        if (evento.changedTouches.length) inicioToque = evento.changedTouches[0].clientX;
    }, { passive: true });
    libro.addEventListener('touchend', (evento) => {
        if (inicioToque === null || !evento.changedTouches.length) return;
        const diferencia = evento.changedTouches[0].clientX - inicioToque;
        inicioToque = null;
        if (Math.abs(diferencia) < 55) return;
        cambiarApertura(indiceActual + (diferencia < 0 ? 1 : -1), diferencia < 0 ? 'adelante' : 'atras');
    }, { passive: true });

    actualizarControles();
})();
