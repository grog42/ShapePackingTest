
//Storage keys
var WriterStorageKey;
var ReaderStorageKey;
var ExampleStorageKey;

//Packer elements
var mainCanvasId;
var sheetWidthInputId;
var sheetHeightInputId;

var dxfInputId;

var packerCanvasScale = 1;

function CanvasResX() {

    return $(sheetWidthInputId).val();
}

function CanvasResY() {

    return $(sheetHeightInputId).val();
}

window.JSFunctions = {

    OnStart: function (ids) {

        //Link Ids
        WriterStorageKey = ids[0];
        ReaderStorageKey = ids[1];
        ExampleStorageKey = ids[2];

        mainCanvasId = ids[3];
        sheetWidthInputId = "#" + ids[4];
        sheetHeightInputId = "#" + ids[5];
        dxfInputId = "#" + ids[6];

        sessionStorage.setItem(WriterStorageKey, "");
        sessionStorage.setItem(ReaderStorageKey, "");

        this.ScaleCanvas(mainCanvasId, CanvasResX(), CanvasResY());

        $.ajax({
            url: "example.txt",
            dataType: "text"
        })
            .done(function (data) {
                sessionStorage.setItem(ExampleStorageKey, data);
            })
            .fail(function (jqXHR, textStatus) {
                console.log("LoadFail:" + textStatus + jqXHR.status);
            });

        $(window).on("resize", function () {

            JSFunctions.ScaleCanvas(mainCanvasId, CanvasResX(), CanvasResY());
        });

        $(sheetWidthInputId).on("change", function () {

            JSFunctions.ScaleCanvas(mainCanvasId, CanvasResX(), CanvasResY());
        });

        $(sheetHeightInputId).on("change", function () {

            JSFunctions.ScaleCanvas(mainCanvasId, CanvasResX(), CanvasResY());
        });

        $(dxfInputId).on("drop", function (e) {
            e.preventDefault();

            document.getElementById(dxfInputId).files = e.dataTransfer.files;

            var changeEvent = new Event('onchange', {
                view: window,
                bubbles: true,
                cancelable: true
            });

            document.getElementById(dxfInputId).dispatchEvent(changeEvent);
        });

        $(dxfInputId).on("dragover", function () {

            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
            e.stopPropagation();
        });

        $(dxfInputId).on("dragenter", function (e) {
            e.preventDefault();
            e.stopPropagation();
        });

        $(dxfInputId).on("dragleave", function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
    },

    //Gets files loaded to the file input
    GetFiles: async function () {

        var files = $(dxfInputId).prop('files');
        var returnText = [];

        for (var i = 0; i < files.length; i++) {

            var file = files[i];

            returnText.push(await toBase64(file));
        }

        $(dxfInputId).val("");

        return returnText;
    },

    ClearCanvas: function (id) {

        var gl = document.getElementById(id).getContext("webgl");
        gl.clearColor(0.0, 0.0, 0.0, 0.0);
        gl.clear(gl.COLOR_BUFFER_BIT);
    },

    DrawCanvas: function (id, lineIndices) {

        var canvas = document.getElementById(id);
        var gl = canvas.getContext('webgl');

        gl.viewport(0, 0, canvas.width, canvas.height);
        gl.clear(gl.COLOR_BUFFER_BIT);

        for (var i = 0; i < lineIndices.length / 2; i++) {

            gl.drawArrays(gl.LINE_STRIP, lineIndices[i * 2], lineIndices[i * 2 + 1]);
        }
    },

    LinkBuffer: function (id, vertexBuffer) {

        var canvas = document.getElementById(id);
        var gl = canvas.getContext('webgl');

        var vertCode =
            'attribute vec3 coordinates;' +

            'void main(void) {' +
            ' gl_Position = vec4(coordinates, 1.0);' +
            '}';

        var fragCode =
            'void main(void) {' +
            ' gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);' +
            '}';

        var vertex_buffer = gl.createBuffer();

        gl.bindBuffer(gl.ARRAY_BUFFER, vertex_buffer);
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertexBuffer), gl.STATIC_DRAW);
        gl.bindBuffer(gl.ARRAY_BUFFER, null);

        var vertShader = gl.createShader(gl.VERTEX_SHADER);

        gl.shaderSource(vertShader, vertCode);
        gl.compileShader(vertShader);

        var fragShader = gl.createShader(gl.FRAGMENT_SHADER);

        gl.shaderSource(fragShader, fragCode);
        gl.compileShader(fragShader);

        var shaderProgram = gl.createProgram();

        gl.attachShader(shaderProgram, vertShader);
        gl.attachShader(shaderProgram, fragShader);
        gl.linkProgram(shaderProgram);
        gl.useProgram(shaderProgram);

        gl.bindBuffer(gl.ARRAY_BUFFER, vertex_buffer);

        var coord = gl.getAttribLocation(shaderProgram, "coordinates");

        gl.vertexAttribPointer(coord, 3, gl.FLOAT, false, 0, 0);
        gl.enableVertexAttribArray(coord);
 
    },

    CreateFile: function (text) {

        var element = document.createElement("a");
        element.style.display = 'none';
        element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
        element.setAttribute('download', "CuttingSheet.dxf");
        document.body.appendChild(element);

        element.click();

        document.body.removeChild(element);
    },

    ScaleCanvas: function (id, resX, resY) {

        var canvas = document.getElementById(id);
        var parent = canvas.parentElement;

        if ((parent.clientHeight - resY) < (parent.clientWidth - resX)){

            canvas.height = parent.clientHeight;
            canvas.width = resX * (parent.clientHeight / resY);

        } else {

            canvas.width = parent.clientWidth;
            canvas.height = resY * (parent.clientWidth / resX);
        }

        packerCanvasScale = canvas.width / resX;
    },

    ClearStorage: function (key) {
        sessionStorage.removeItem(key);
        console.log("Storage " + key + " cleared");
    },

    AppendToStorage: function (key, value) {

        if (sessionStorage.getItem(key) === null) {

            sessionStorage.setItem(key, value);
            console.log("Storage " + key + " created");
        } else {

            var str = sessionStorage.getItem(key);
            
            sessionStorage.setItem(key, str + value);

            console.log("Storage " + key + " extended");
        }
    },

    //Takes dxf data stored in session storage and writes it up in a file
    StorageToFile: function (key) {

        if (sessionStorage.getItem(key) !== null) {

            this.CreateFile(sessionStorage.getItem(key));

        } else {
            console.log("File is null");
        }
        
    },

    GetFromStroage: function (key) {

        return sessionStorage.getItem(key);
    }
};

const toBase64 = file => new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.readAsText(file);
    reader.onload = () => resolve(reader.result);
    reader.onerror = error => reject(error);
});