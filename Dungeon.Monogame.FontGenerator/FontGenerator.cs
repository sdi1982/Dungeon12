﻿using Dungeon.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dungeon.Monogame
{
    public class FontGenerator
    {
        public FontGenerator(string contentFilePath, string fontName, int minSize = 8, int maxSize = 72)
        {
            this.contentFilePath = contentFilePath;
            contentDirectory = Path.Combine(Path.GetDirectoryName(contentFilePath), "fonts");
            contentContent = File.ReadAllText(contentFilePath);

            if (fontName.Contains(","))
            {
                fontNames = fontName.Split(",", StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                fontNames = new string[] { fontName };
            }

            if (minSize > 0)
            {
                min = minSize;
            }
            if (maxSize > 0 && maxSize < 200)
            {
                max = maxSize;
            }
        }

        int min = 8;
        int max = 72;
        string[] fontNames;
        string contentDirectory;
        string contentContent;
        string contentFilePath;

        public void Generate()
        {
            var spritefontTemplate = ResourceLoader.Load("template.spritefont".AsmNameRes(),true).Stream.AsString();
            var contentTemplate = ResourceLoader.Load("content.template".AsmNameRes(),true).Stream.AsString();

            foreach (var fontName in fontNames)
            {
                var fontFolderName = Path.Combine(contentDirectory, fontName);
                if (!Directory.Exists(fontFolderName))
                {
                    Directory.CreateDirectory(fontFolderName);
                }

                for (int size = min; size < max + 1; size++)
                {
                    var spriteFont = spritefontTemplate.Replace("{FontName}", fontName)
                        .Replace("{FontSize}", size.ToString());

                    var content = contentTemplate.Replace("{FontName}", fontName)
                        .Replace("{FontSize}", size.ToString());

                    var spritefontPath = Path.Combine(fontFolderName, $"{fontName}{size}.spritefont");

                    File.WriteAllText(spritefontPath, spriteFont);

                    contentContent += Environment.NewLine + content;
                }
            }            

            File.WriteAllText(contentFilePath, contentContent);
        }
    }
}
