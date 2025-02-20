using System;
using System.Text;
using System.Text.RegularExpressions;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using NetTopologySuite.Utilities;

namespace com.antlersoft.HostedTools.Geography
{
    public static class WellKnownCoordinates
    {
        // ReSharper disable InconsistentNaming
        private const string MULTIPOLYGON = "MULTIPOLYGON";
        private const string POLYGON = "POLYGON";
        private const string GEOMETRYCOLLECTION = "GEOMETRYCOLLECTION";
        // ReSharper restore InconsistentNaming
        private static readonly Regex PointRegex = new Regex("POINT *\\((-?[0-9]*\\.?[0-9]*) *(-?[0-9]*\\.?[0-9]*)[^)]*\\)", RegexOptions.IgnoreCase);
        enum State
        {
            Initial,
            ExpectingSomeGeometry,
            ExpectingMultipolygon,
            ExpectingPolygon,
            ExpectingRing,
            ExpectingCoordinate,
            InNumber,
            EndRing,
            EndPolygon,
            Complete
        }

        static bool UpperStartsWith(string val, int offset, string s)
        {
            return val.Length - offset >= s.Length && val.Substring(offset, s.Length).ToUpperInvariant() == s;
        }

        static void ReportError(string message, string val, int offset)
        {
            throw new ArgumentException(String.Format("{2} at {0}: {1}", offset, val, message));
        }

        public static GeoJsonGeometry GetCoordinatesFromWellKnownText(string val)
        {
            JsonHtValue coordinates = new JsonHtValue();
            
            string? type = null;
            State state = State.Initial;
            int len = val.Length;
            JsonHtValue? currentCollection = null;
            int ccolIndex = 0;
            JsonHtValue? currentCoordinate = null;
            int ccIndex = 0;
            JsonHtValue? currentRing = null;
            int crIndex = 0;
            JsonHtValue? currentPolygon = null;
            int cpIndex = 0;
            JsonHtValue? currentMultipolygon = null;
            int mpIndex = 0;
            StringBuilder? currentNumber = null;

            for (int offset = 0; offset < len; offset++)
            {
                char c = val[offset];
                switch (state)
                {
                    case State.Initial:
                        if (! Char.IsWhiteSpace(c))
                        {                            
                            if (UpperStartsWith(val, offset, MULTIPOLYGON))
                            {
                                offset += MULTIPOLYGON.Length;
                                state = State.ExpectingMultipolygon;
                                type = "MultiPolygon";
                                currentMultipolygon = new JsonHtValue();
                                coordinates = currentMultipolygon;
                            }
                            else if (UpperStartsWith(val, offset, POLYGON))
                            {
                                offset += POLYGON.Length;
                                state = State.ExpectingPolygon;
                                type = "Polygon";
                                currentPolygon = new JsonHtValue();
                                coordinates = currentPolygon;
                            }
                            else if (UpperStartsWith(val, offset, GEOMETRYCOLLECTION))
                            {
                                offset += GEOMETRYCOLLECTION.Length;
                                state = State.ExpectingSomeGeometry;
                                type = "MultiPolygon";
                                currentCollection = new JsonHtValue();
                                coordinates = currentCollection;
                                for (; val[offset++] != '(';)
                                {
                                }
                            }
                            else
                            {
                                var match = PointRegex.Match(val);
                                if (match.Success)
                                {
                                    coordinates[0] = new JsonHtValue(Double.Parse(match.Groups[1].Value));
                                    coordinates[1] = new JsonHtValue(Double.Parse(match.Groups[2].Value));
                                    return new GeoJsonGeometry() {type = "Point", coordinates = coordinates};
                                }
                                throw new ArgumentException(
                                    "Only POLYGON, MULTIPOLYGON, GEOMETRYCOLLECTION of POLYGON, MULTIPOLYGON or POINT supported");
                            }
                        }
                        break;
                    case State.ExpectingSomeGeometry:
                        if (! Char.IsWhiteSpace(c))
                        {
                            if (c == ')')
                            {
                                state = State.Complete;
                            }
                            else if (UpperStartsWith(val, offset, MULTIPOLYGON))
                            {
                                offset += MULTIPOLYGON.Length;
                                state = State.ExpectingMultipolygon;
                                currentMultipolygon = new JsonHtValue();
                                mpIndex = 0;
                            }
                            else if (UpperStartsWith(val, offset, POLYGON))
                            {
                                offset += POLYGON.Length;
                                state = State.ExpectingPolygon;
                                currentPolygon = new JsonHtValue();
                                cpIndex = 0;
                            }
                            else
                            {
                                for (int parenCount = -1; parenCount != 0; offset++)
                                {
                                    if (val[offset] == '(')
                                    {
                                        if (parenCount == -1)
                                        {
                                            parenCount = 1;
                                        }
                                        else
                                        {
                                            parenCount++;
                                        }
                                    }
                                    else if (val[offset] == ')')
                                    {
                                        parenCount--;
                                    }
                                }
                            }
                        }
                        break;
                    case State.ExpectingMultipolygon:
                        if (! Char.IsWhiteSpace(c))
                        {
                            if (c != '(')
                            {
                                ReportError("Expecting (", val, offset);
                            }
                            else
                            {
                                state = State.ExpectingPolygon;
                                currentPolygon = new JsonHtValue();
                                cpIndex = 0;
                            }
                        }
                        break;
                    case State.ExpectingPolygon:
                        if (!Char.IsWhiteSpace(c))
                        {
                            if (c != '(')
                            {
                                ReportError("Expecting (", val, offset);
                            }
                            else
                            {
                                state = State.ExpectingRing;
                                currentRing = new JsonHtValue();
                                crIndex = 0;
                            }
                        }
                        break;
                    case State.ExpectingRing:
                        if (!Char.IsWhiteSpace(c))
                        {
                            if (c != '(')
                            {
                                ReportError("Expecting (", val, offset);
                            }
                            else
                            {
                                state = State.ExpectingCoordinate;
                                currentCoordinate = new JsonHtValue();
                                ccIndex = 0;
                            }
                        }
                        break;
                    case State.ExpectingCoordinate:
                        if (!Char.IsWhiteSpace(c))
                        {
                            if (c == ')' || c==',')
                            {
                                if (ccIndex < 2)
                                {
                                    ReportError("At least 2 coordinates required", val, offset);
                                }
                                if (currentRing==null) {
                                    throw new AssertionFailedException("Current ring null when expecting coordinates");
                                }
                                currentRing[crIndex++] = currentCoordinate;
                                ccIndex = 0;
                                currentCoordinate = new JsonHtValue();
                                if (c != ',')
                                {
                                    state = State.EndRing;
                                    if (currentPolygon == null) {
                                        throw new AssertionFailedException("Current polygon is null when expecting coordinates");
                                    }
                                    currentPolygon[cpIndex++] = currentRing;
                                    crIndex = 0;
                                    currentRing = new JsonHtValue();
                                }
                            }
                            else
                            {
                                currentNumber = new StringBuilder();
                                currentNumber.Append(c);
                                state = State.InNumber;
                            }
                        }
                        break;
                    case State.InNumber:
                        if (currentNumber == null) {
                            throw new AssertionFailedException("No currentNumber while InNumber");
                        }
                        if (Char.IsWhiteSpace(c) || c == ')' || c == ',')
                        {
                            double v;
                            if (! Double.TryParse(currentNumber.ToString(), out v))
                            {
                                ReportError("Invalid number: "+currentNumber, val, offset);
                            }
                            if (currentCoordinate == null) {
                                throw new AssertionFailedException("No currentCoordinate while InNumber");
                            }
                            currentCoordinate[ccIndex++] = new JsonHtValue(v);
                            offset--; // Read that character again
                            state = State.ExpectingCoordinate;
                        }
                        else
                        {
                            currentNumber.Append(c);
                        }
                        break;
                    case State.EndRing:
                        if (! Char.IsWhiteSpace(c))
                        {
                            if (c == ',')
                            {
                                state = State.ExpectingRing;
                            }
                            else if (c == ')')
                            {
                                state = State.EndPolygon;
                            }
                            else
                            {
                                ReportError("Unexpected character after ring", val, offset);
                            }
                        }
                        break;
                    case State.EndPolygon:
                        if (! Char.IsWhiteSpace(c))
                        {
                            if (c == ',' || c == ')')
                            {
                                if (currentMultipolygon == null && currentCollection == null)
                                {
                                    ReportError("Multiple polygons when expecting one", val, offset);
                                }
                                if (currentCollection == null)
                                {
                                    if (currentMultipolygon != null)
                                    {
                                        currentMultipolygon[mpIndex++] = currentPolygon;
                                    }
                                    if (c == ',')
                                    {
                                        currentPolygon = new JsonHtValue();
                                        cpIndex = 0;
                                        state = State.ExpectingPolygon;
                                    }
                                    else
                                    {
                                        state = State.Complete;
                                    }
                                }
                                else
                                {
                                    if (currentMultipolygon != null)
                                    {
                                        currentMultipolygon[mpIndex++] = currentPolygon;
                                        if (c == ',')
                                        {
                                            currentPolygon = new JsonHtValue();
                                            cpIndex = 0;
                                            state = State.ExpectingPolygon;
                                        }
                                        else
                                        {
                                            foreach (var poly in currentMultipolygon.AsArrayElements)
                                            {
                                                currentCollection[ccolIndex++] = poly;
                                            }
                                            state = State.ExpectingSomeGeometry;
                                        }
                                    }
                                    else
                                    {
                                        currentCollection[ccolIndex++] = currentPolygon;
                                        if (c == ',')
                                        {
                                            state = State.ExpectingSomeGeometry;
                                        }
                                        else
                                        {
                                            state = State.Complete;
                                        }
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            if (type == null) {
                throw new AssertionFailedException("WellKnownCoordinates end with unknown type");
            }
            return new GeoJsonGeometry() {type = type, coordinates = coordinates};
        }
    }
}
