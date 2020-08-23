using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace WebApps
{
    /// <summary>
    /// Object which acts as a interface for java script functions
    /// </summary>
    public class JsInteropHandler
    {
        private IJSRuntime jsRuntime;

        public JsInteropHandler(IJSRuntime _JSRuntime)
        {
            jsRuntime = _JSRuntime;
        }

        public void InitializeJS()
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.OnStart", (object) IdList.ToArray());
        }

        public async Task<string[]> GetFiles()
        {
            return await jsRuntime.InvokeAsync<string[]>("JSFunctions.GetFiles", new TimeSpan(0, 0, 2));
        }

        public async Task<string> GetTextFile(string path)
        {
            return await jsRuntime.InvokeAsync<string>("JSFunctions.GetTextFile", new TimeSpan(0, 0, 2), path);
        }

        public void ClearCanvas(string id)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.ClearCanvas", id);
        }

        public void DrawCanvas(string id, int[] indices)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.DrawCanvas", id, (object)indices);
        }

        public void DrawBitmap(string id, float[] vertexBuffer)
        {
             ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.DrawBitmap", id, (object) vertexBuffer);
        }

        public void LinkVertexBuffer(string id, float[] buffer)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.LinkBuffer", id, (object)buffer);
        }

        public async Task<float> GetPackerCanvasScale()
        {
            return await jsRuntime.InvokeAsync<float>("JSFunctions.GetPackerCanvasScale", new TimeSpan(0, 0, 2));
        }

        public void ClearFileStroage(string key)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.ClearStorage", key);
        }

        public void AppendToStorage(string key, string value)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.AppendToStorage", key, value);
        }

        public void StorageToFile(string key)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("JSFunctions.StorageToFile", key);
        }

        public async Task<string> GetFromStroage(string key)
        {
            return await jsRuntime.InvokeAsync<string>("JSFunctions.GetFromStroage", new TimeSpan(0, 0, 2), key);
        }

        public void ConsoleLog(object str)
        {
            ((IJSInProcessRuntime)jsRuntime).Invoke<object>("console.log", str);
        }
    }
}
