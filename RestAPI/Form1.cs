using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using RestSharp;

namespace RestAPI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var client = new RestClient("https://eastasia.api.cognitive.microsoft.com/face/v1.0/detect?returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender");
            var request = new RestRequest(Method.POST);
            request.AddHeader("Postman-Token", "c56047e2-9990-48ff-8aac-0e60bf3b8b43");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Ocp-Apim-Subscription-Key", "3122cabdd3314337be175e365f579cbb");
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("undefined", "{\n    \"url\": \"http://www.hkcinemagic.com/en/images/movie/large/DragonFromRussia-SamHui2_31cb979425c67429d5865d3a3b65b8ca.jpg\"\n}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            var content = response.Content;

            // easy async support
            //client.ExecuteAsync(request, response => {
            //    Console.WriteLine(response.Content);
            //});

            //// async with deserialization
            //var asyncHandle = client.ExecuteAsync<Person>(request, response => {
            //    Console.WriteLine(response.Data.Name);
            //});

        }
    }
}
