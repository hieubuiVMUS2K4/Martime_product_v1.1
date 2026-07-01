using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace MaritimeEdge.Helpers
{
    public class DocxHeaderFooterExtractor
    {
        public static (string HeaderHtml, string FooterHtml) Extract(Stream docxStream)
        {
            string headerHtml = "";
            string footerHtml = "";

            try
            {
                using (var archive = new ZipArchive(docxStream, ZipArchiveMode.Read, leaveOpen: true))
                {
                    // Find headers and footers in the ZIP archive
                    var headerEntries = archive.Entries
                        .Where(e => e.FullName.StartsWith("word/header", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    var footerEntries = archive.Entries
                        .Where(e => e.FullName.StartsWith("word/footer", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    // Process the main header (pick the largest entry by size to get the detailed version)
                    var mainHeaderEntry = headerEntries.OrderByDescending(e => e.Length).FirstOrDefault();
                    if (mainHeaderEntry != null)
                    {
                        headerHtml = ConvertXmlToHtml(archive, mainHeaderEntry);
                    }

                    // Process the main footer
                    var mainFooterEntry = footerEntries.OrderByDescending(e => e.Length).FirstOrDefault();
                    if (mainFooterEntry != null)
                    {
                        footerHtml = ConvertXmlToHtml(archive, mainFooterEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting docx headers/footers: {ex.Message}");
            }

            return (headerHtml, footerHtml);
        }

        private static string ConvertXmlToHtml(ZipArchive archive, ZipArchiveEntry entry)
        {
            try
            {
                XDocument doc;
                using (var stream = entry.Open())
                {
                    doc = XDocument.Load(stream);
                }

                // Load relationships inside the header/footer entry to link image paths
                var rels = new Dictionary<string, string>();
                var relsEntryName = $"word/_rels/{entry.Name}.rels";
                var relsEntry = archive.GetEntry(relsEntryName);
                if (relsEntry != null)
                {
                    using (var relStream = relsEntry.Open())
                    {
                        var relDoc = XDocument.Load(relStream);
                        XNamespace relNs = "http://schemas.openxmlformats.org/package/2006/relationships";
                        foreach (var rel in relDoc.Descendants(relNs + "Relationship"))
                        {
                            var id = rel.Attribute("Id")?.Value;
                            var target = rel.Attribute("Target")?.Value;
                            if (id != null && target != null)
                            {
                                if (target.StartsWith("media/"))
                                {
                                    rels[id] = "word/" + target;
                                }
                                else if (target.StartsWith("/word/media/"))
                                {
                                    rels[id] = target.TrimStart('/');
                                }
                            }
                        }
                    }
                }

                // Standard OpenXML namespaces
                XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
                XNamespace r = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
                XNamespace a = "http://schemas.openxmlformats.org/drawingml/2006/main";

                var body = doc.Root;
                if (body == null) return "";

                return ParseNode(body, archive, rels, w, r, a);
            }
            catch
            {
                return "";
            }
        }

        private static string ParseNode(XElement element, ZipArchive archive, Dictionary<string, string> rels, XNamespace w, XNamespace r, XNamespace a)
        {
            if (element.Name == w + "tbl")
            {
                var rowsHtml = "";
                foreach (var row in element.Elements(w + "tr"))
                {
                    rowsHtml += ParseNode(row, archive, rels, w, r, a);
                }
                return $"<table class=\"docx-header-table\" style=\"width:100%; border-collapse:collapse; border:1px solid #cbd5e1; margin-bottom:12px; font-size:11px; text-align:left;\">{rowsHtml}</table>";
            }
            else if (element.Name == w + "tr")
            {
                var cellsHtml = "";
                foreach (var cell in element.Elements(w + "tc"))
                {
                    cellsHtml += ParseNode(cell, archive, rels, w, r, a);
                }
                return $"<tr>{cellsHtml}</tr>";
            }
            else if (element.Name == w + "tc")
            {
                var content = "";
                foreach (var child in element.Elements())
                {
                    content += ParseNode(child, archive, rels, w, r, a);
                }
                return $"<td style=\"border:1px solid #cbd5e1; padding:6px; vertical-align:middle;\">{content}</td>";
            }
            else if (element.Name == w + "p")
            {
                var content = "";
                foreach (var child in element.Elements())
                {
                    content += ParseNode(child, archive, rels, w, r, a);
                }
                if (string.IsNullOrWhiteSpace(content)) return "";
                return $"<p style=\"margin:2px 0; line-height:1.2;\">{content}</p>";
            }
            else if (element.Name == w + "r")
            {
                var content = "";
                foreach (var child in element.Elements())
                {
                    content += ParseNode(child, archive, rels, w, r, a);
                }

                var rPr = element.Element(w + "rPr");
                if (rPr != null)
                {
                    if (rPr.Element(w + "b") != null)
                    {
                        content = $"<strong>{content}</strong>";
                    }
                    if (rPr.Element(w + "i") != null)
                    {
                        content = $"<em>{content}</em>";
                    }
                }
                return content;
            }
            else if (element.Name == w + "t")
            {
                return WebUtility.HtmlEncode(element.Value);
            }
            else if (element.Name == w + "drawing" || element.Name.LocalName == "drawing")
            {
                var blip = element.Descendants().FirstOrDefault(d => d.Name.LocalName == "blip");
                if (blip != null)
                {
                    var embedId = blip.Attribute(r + "embed")?.Value ?? blip.Attribute(blip.Name.Namespace + "embed")?.Value;
                    if (embedId != null && rels.TryGetValue(embedId, out var mediaPath))
                    {
                        var mediaEntry = archive.GetEntry(mediaPath);
                        if (mediaEntry != null)
                        {
                            using (var mStream = mediaEntry.Open())
                            using (var ms = new MemoryStream())
                            {
                                mStream.CopyTo(ms);
                                var bytes = ms.ToArray();
                                var base64 = Convert.ToBase64String(bytes);
                                var mimeType = mediaPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png" : "image/jpeg";
                                return $"<img src=\"data:{mimeType};base64,{base64}\" style=\"max-height:45px; display:inline-block; vertical-align:middle;\" />";
                            }
                        }
                    }
                }
                return "";
            }
            else
            {
                var content = "";
                foreach (var child in element.Elements())
                {
                    content += ParseNode(child, archive, rels, w, r, a);
                }
                return content;
            }
        }
    }
}
