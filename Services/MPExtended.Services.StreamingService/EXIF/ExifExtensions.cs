#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using MetadataExtractor;

namespace MPExtended.Services.StreamingService.EXIF
{
  public static class ExifExtensions
  {
    #region Helper Methods

    public static string ToMapString(this double value)
    {
      return value.ToString(CultureInfo.InvariantCulture);
    }

    public static string ToFileName(this double value)
    {
      byte[] valBytes = BitConverter.GetBytes(value);
      return BitConverter.ToString(valBytes).Replace("-", string.Empty);
    }

    public static string ToLatitudeString(this double tag)
    {
      if (tag == 0)
      {
        return GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
      }
      return (tag > 0 ? "N" : "S") + GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
    }

    public static string ToLongitudeString(this double tag)
    {
      if (tag == 0)
      {
        return GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
      }
      return (tag > 0 ? "W" : "E") + GeoLocation.DecimalToDegreesMinutesSecondsString(tag);
    }

    public static string ToAltitudeString(this double tag)
    {
      return tag == 0 ? "Sea level" : String.Format(tag > 0 ? "Sea level {0} metres" : "Below sea level {0} metres", Math.Round(tag, 2));
    }

    public static int ToRotation(this int orientation)
    {
      if (orientation == 6)
      {
        return 1; // 90 degree:  112/03/06 00
      }
      if (orientation == 3)
      {
        return 2; // 180 degree: 112/03/03 00
      }
      if (orientation == 8)
      {
        return 3; // 270 degree: 112/03/08 00
      }
      return 0; // not rotated
    }

    #endregion

    #region Exif Properties

    public static string ToString(this ExifMetadata.Metadata metadata)
    {
      string full = string.Empty;
      Type type = typeof(ExifMetadata.Metadata);
      foreach (FieldInfo prop in type.GetFields())
      {
        string value = string.Empty;
        string caption = prop.Name;
        switch (prop.Name)
        {
          case nameof(ExifMetadata.Metadata.ImageDimensions):
            value = metadata.ImageDimensionsAsString();
            break;
          case nameof(ExifMetadata.Metadata.Resolution):
            value = metadata.ResolutionAsString();
            break;
          case nameof(ExifMetadata.Metadata.Location):
            if (metadata.Location != null)
            {
              string latitude = metadata.Location.Latitude.ToLatitudeString() ?? string.Empty;
              string longitude = metadata.Location.Longitude.ToLongitudeString() ?? string.Empty;
              if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
              {
                value = latitude + " / " + longitude;
              }
            }
            break;
          case nameof(ExifMetadata.Metadata.Altitude):
            if (metadata.Location != null)
            {
              value = metadata.Altitude.ToAltitudeString();
            }
            break;
          case nameof(ExifMetadata.Metadata.HDR):
            continue;
          default:
            value = ((ExifMetadata.MetadataItem)prop.GetValue(metadata)).DisplayValue ?? string.Empty;
            break;
        }
        if (!string.IsNullOrEmpty(value))
        {
          full = full + caption + ": " + value + "\n";
        }
      }
      return full;
    }

    public static string ToShortString(this ExifMetadata.Metadata metadata)
    {
      string exifoutline = string.Empty;
      exifoutline = exifoutline + (!string.IsNullOrEmpty(metadata.EquipmentMake.DisplayValue) ? metadata.EquipmentMake.DisplayValue + " " : string.Empty);
      exifoutline = exifoutline + (!string.IsNullOrEmpty(metadata.CameraModel.DisplayValue) ? metadata.CameraModel.DisplayValue + " " : string.Empty);
      exifoutline = exifoutline + (!string.IsNullOrEmpty(metadata.ViewerComments.DisplayValue) ? metadata.ViewerComments.DisplayValue + " " : string.Empty);
      return exifoutline;
    }

    public static Dictionary<string, string> GetExifList(this ExifMetadata.Metadata metadata)
    {
      Dictionary<string, string> exif = new Dictionary<string, string>();

      Type type = typeof(ExifMetadata.Metadata);
      foreach (FieldInfo prop in type.GetFields())
      {
        string value = string.Empty;
        string caption = prop.Name;
        switch (prop.Name)
        {
          case nameof(ExifMetadata.Metadata.ImageDimensions):
            value = metadata.ImageDimensionsAsString();
            break;
          case nameof(ExifMetadata.Metadata.Resolution):
            value = metadata.ResolutionAsString();
            break;
          case nameof(ExifMetadata.Metadata.Location):
            if (metadata.Location != null)
            {
              string latitude = metadata.Location.Latitude.ToLatitudeString() ?? string.Empty;
              string longitude = metadata.Location.Longitude.ToLongitudeString() ?? string.Empty;
              if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
              {
                value = latitude + " / " + longitude;
              }
            }
            break;
          case nameof(ExifMetadata.Metadata.Altitude):
            if (metadata.Location != null)
            {
              value = metadata.Altitude.ToAltitudeString();
            }
            break;
          case nameof(ExifMetadata.Metadata.HDR):
            continue;
          default:
            value = ((ExifMetadata.MetadataItem)prop.GetValue(metadata)).DisplayValue ?? string.Empty;
            break;
        }
        if (!string.IsNullOrEmpty(value))
        {
          exif.Add(caption, value);
        }
      }

      return exif;
    }

    #endregion
  }
}
