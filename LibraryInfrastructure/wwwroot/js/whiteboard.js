
(() => {
    const canvas = document.getElementById('boardCanvas');
    const notesContainer = document.getElementById('notesContainer');
    const colorPicker = document.getElementById('colorPicker');
    const sizePicker = document.getElementById('sizePicker');
    const btnPen = document.getElementById('btnPen');
    const btnClear = document.getElementById('btnClear');
    const btnAddNote = document.getElementById('btnAddNote');
    const ctx = canvas.getContext('2d');

    let strokes = [];
    let drawing = false;
    let last = null;
    function resizeCanvas() {
        canvas.width = canvas.parentElement.clientWidth;
        canvas.height = canvas.parentElement.clientHeight;
        redrawFromBuffer();
    }

    window.addEventListener('resize', resizeCanvas);
    resizeCanvas();
    function redrawFromBuffer() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        strokes.forEach(s => drawStrokeOnCanvas(s, false));
    }

    function drawStrokeOnCanvas(s, saveLocal = true) {
        ctx.strokeStyle = s.color;
        ctx.lineWidth = s.width;
        ctx.lineCap = 'round';

        ctx.beginPath();
        ctx.moveTo(s.x1, s.y1);
        ctx.lineTo(s.x2, s.y2);
        ctx.stroke();

        if (saveLocal) strokes.push(s);
    }
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/boardHub")
        .withAutomaticReconnect()
        .build();

    connection.start()
        .then(() => console.log("SignalR connected"))
        .catch(err => console.error(err));

    connection.on("ReceiveStroke", stroke => {
        drawStrokeOnCanvas(stroke, false);
    });
    connection.on("BoardCleared", () => {
        strokes = [];
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        notesContainer.innerHTML = "";
    });
    canvas.addEventListener('pointerdown', e => {
        drawing = true;
        last = { x: e.offsetX, y: e.offsetY };
    });

    canvas.addEventListener('pointermove', e => {
        if (!drawing) return;

        const current = { x: e.offsetX, y: e.offsetY };

        const stroke = {
            x1: last.x, y1: last.y,
            x2: current.x, y2: current.y,
            color: colorPicker.value,
            width: parseInt(sizePicker.value || "2")
        };

        drawStrokeOnCanvas(stroke);
        connection.invoke("Draw", stroke).catch(console.error);

        last = current;
    });

    window.addEventListener('pointerup', () => {
        drawing = false;
        last = null;
    });
    function clearCanvasLocal() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        strokes = [];
    }

    btnClear.addEventListener('click', () => {
        if (!confirm("Clear the board for everyone?")) return;
        clearCanvasLocal();
        connection.invoke("ClearBoard").catch(console.error);
    });
    let nextNoteId = Date.now();

    btnAddNote.addEventListener('click', () => {
        const note = {
            id: ++nextNoteId,
            text: "New note",
            left: 50,
            top: 50,
            color: "#fff8b0"
        };

        createNoteElement(note, true);
        connection.invoke("AddNote", note).catch(console.error);
    });

    connection.on("NoteAdded", note => createNoteElement(note));
    connection.on("NoteUpdated", note => updateNoteText(note));
    connection.on("NoteMoved", note => moveNote(note));
    connection.on("NoteDeleted", id => removeNote(id));
    function createNoteElement(note, focus = false) {
        const el = document.createElement('div');
        el.className = 'note';
        el.dataset.id = note.id;

        el.style.left = note.left + "px";
        el.style.top = note.top + "px";
        el.style.background = note.color || "#fff8b0";

        el.innerHTML = `
            <div class="note-header">
                <button class="note-delete">✕</button>
            </div>
            <div class="note-text" contenteditable="true">${escapeHtml(note.text)}</div>
        `;

        notesContainer.appendChild(el);
        if (focus) {
            const txt = el.querySelector(".note-text");
            setTimeout(() => {
                txt.focus();
                placeCaretAtEnd(txt);
            }, 40);
        }
        el.querySelector(".note-delete").addEventListener("click", () => {
            const id = parseInt(el.dataset.id);
            el.remove();
            connection.invoke("DeleteNote", id).catch(console.error);
        });
        const textDiv = el.querySelector(".note-text");
        let timer = null;

        textDiv.addEventListener("input", () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                connection.invoke("UpdateNote", {
                    id: parseInt(el.dataset.id),
                    text: textDiv.innerText
                });
            }, 400);
        });
        makeDraggable(el, (left, top) => {
            connection.invoke("MoveNote", {
                id: parseInt(el.dataset.id),
                left, top
            });
        });
    }

    function updateNoteText(note) {
        const el = document.querySelector(`.note[data-id='${note.id}']`);
        if (el) el.querySelector(".note-text").innerText = note.text;
    }

    function moveNote(note) {
        const el = document.querySelector(`.note[data-id='${note.id}']`);
        if (el) {
            el.style.left = note.left + "px";
            el.style.top = note.top + "px";
        }
    }

    function removeNote(id) {
        const el = document.querySelector(`.note[data-id='${id}']`);
        if (el) el.remove();
    }
    function makeDraggable(el, onMove) {
        let dragging = false;
        let startX, startY, baseLeft, baseTop;

        el.addEventListener("pointerdown", e => {
            if (e.target.closest('.note-text')) return;

            dragging = true;
            el.setPointerCapture(e.pointerId);

            startX = e.clientX;
            startY = e.clientY;
            baseLeft = parseInt(el.style.left);
            baseTop = parseInt(el.style.top);

            el.classList.add('dragging');
        });

        el.addEventListener("pointermove", e => {
            if (!dragging) return;

            const dx = e.clientX - startX;
            const dy = e.clientY - startY;

            const left = baseLeft + dx;
            const top = baseTop + dy;

            el.style.left = left + "px";
            el.style.top = top + "px";
        });

        el.addEventListener("pointerup", e => {
            if (!dragging) return;

            dragging = false;
            el.releasePointerCapture(e.pointerId);
            el.classList.remove('dragging');

            onMove(parseInt(el.style.left), parseInt(el.style.top));
        });
    }
    function placeCaretAtEnd(el) {
        const range = document.createRange();
        range.selectNodeContents(el);
        range.collapse(false);

        const sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    }

    function escapeHtml(str) {
        return (str || "").replace(/[&<>"']/g, m =>
            ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[m]
        );
    }

})();
