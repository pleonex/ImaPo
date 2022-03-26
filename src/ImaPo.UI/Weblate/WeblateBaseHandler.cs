// Copyright (c) 2022 Benito Palacios Sánchez

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using ImaPo.UI.Weblate.Models;

namespace ImaPo.UI.Weblate;

public abstract class WeblateBaseHandler
{
    protected WeblateBaseHandler(HttpClient client)
    {
        Client = client;

        JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) {
            PropertyNamingPolicy = Yoh.Text.Json.NamingPolicies.JsonNamingPolicies.SnakeLowerCase,
        };
    }

    protected HttpClient Client { get; }

    protected JsonSerializerOptions JsonOptions { get; }

    protected async IAsyncEnumerable<T> FetchListAsync<T>(string requestRelativeUri)
    {
        var requestUri = new Uri(requestRelativeUri, UriKind.Relative);
        bool moreRequests = false;
        do {
            Stream result = await Client.GetStreamAsync(requestUri).ConfigureAwait(false);
            var response = await JsonSerializer.DeserializeAsync<ResponseList<T>>(result, JsonOptions)
                .ConfigureAwait(false);
            if (response is null) {
                yield break;
            }

            foreach (T screenshot in response.Results) {
                yield return screenshot;
            }

            moreRequests = !string.IsNullOrEmpty(response.Next);
            if (moreRequests) {
                requestUri = new Uri(response.Next);
            }
        } while (moreRequests);
    }

    protected T DeserializeJson<T>(string json) => JsonSerializer.Deserialize<T>(json, JsonOptions);
}
