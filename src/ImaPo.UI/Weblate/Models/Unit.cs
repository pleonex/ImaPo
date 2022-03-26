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
using System.Collections.ObjectModel;

namespace ImaPo.UI.Weblate.Models;

public class Unit
{
    public string Translation { get; set; }

    public Collection<string> Source { get; set; } = new Collection<string>();

    public string PreviousSource { get; set; }

    public Collection<string> Target { get; set; } = new Collection<string>();

    public long IdHash { get; set; }

    public long ContentHash { get; set; }

    public string Location { get; set; }

    public string Context { get; set; }

    public string Note { get; set; }

    public string Flags { get; set; }

    public int State { get; set; }

    public bool Fuzzy { get; set; }

    public bool Translated { get; set; }

    public bool Approved { get; set; }

    public int Position { get; set; }

    public bool HasSuggestion { get; set; }

    public bool HasComment { get; set; }

    public bool HasFailingCheck { get; set; }

    public int NumWords { get; set; }

    public int Priority { get; set; }

    public int Id { get; set; }

    public string Explanation { get; set; }

    public string ExtraFlags { get; set; }

    public string WebUrl { get; set; }

    public string SourceUnit { get; set; }

    public bool Pending { get; set; }
}
