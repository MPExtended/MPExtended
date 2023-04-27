using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPExtended.Services.StreamingService.MediaInfo
{
  public static class MediaInfoExtenstions
  {
    #region mappings

    private static readonly Dictionary<global::MediaInfo.Model.AudioCodec, string> _audioCodecs = new Dictionary<global::MediaInfo.Model.AudioCodec, string>()
    {
      { global::MediaInfo.Model.AudioCodec.Undefined, string.Empty },
      { global::MediaInfo.Model.AudioCodec.MpegLayer1, "MP1" },
      { global::MediaInfo.Model.AudioCodec.MpegLayer2, "MP2" },
      { global::MediaInfo.Model.AudioCodec.MpegLayer3, "MP3" },
      { global::MediaInfo.Model.AudioCodec.PcmIntBig, "PCM" },
      { global::MediaInfo.Model.AudioCodec.PcmIntLit, "PCM" },
      { global::MediaInfo.Model.AudioCodec.PcmFloatIeee, "PCM" },
      { global::MediaInfo.Model.AudioCodec.Ac3, "AC-3" },
      { global::MediaInfo.Model.AudioCodec.Ac3Atmos, "AC-3 ATMOS" },
      { global::MediaInfo.Model.AudioCodec.Ac3Bsid9, "AC-3 NET" },
      { global::MediaInfo.Model.AudioCodec.Ac3Bsid10, "AC-3 NET" },
      { global::MediaInfo.Model.AudioCodec.Eac3, "EAC3" },
      { global::MediaInfo.Model.AudioCodec.Eac3Atmos, "EAC3 ATMOS" },
      { global::MediaInfo.Model.AudioCodec.Truehd, "TRUEHD" },
      { global::MediaInfo.Model.AudioCodec.TruehdAtmos, "TRUEHD ATMOS" },
      { global::MediaInfo.Model.AudioCodec.Dts, "DTS" },
      { global::MediaInfo.Model.AudioCodec.DtsX, "DTSX" },
      { global::MediaInfo.Model.AudioCodec.DtsHdMa, "DTSHD_MA" },
      { global::MediaInfo.Model.AudioCodec.DtsExpress, "DTSEX" },
      { global::MediaInfo.Model.AudioCodec.DtsHdHra, "DTSHD_HRA" },
      { global::MediaInfo.Model.AudioCodec.DtsEs, "DTS ES" },
      { global::MediaInfo.Model.AudioCodec.DtsHd, "DTSHD" },
      { global::MediaInfo.Model.AudioCodec.Flac, "FLAC" },
      { global::MediaInfo.Model.AudioCodec.Opus, "OPUS" },
      { global::MediaInfo.Model.AudioCodec.Tta1, "TTA1" },
      { global::MediaInfo.Model.AudioCodec.Vorbis, "VORBIS" },
      { global::MediaInfo.Model.AudioCodec.WavPack4, "WAVPACK4" },
      { global::MediaInfo.Model.AudioCodec.WavPack, "WAVPACK" },
      { global::MediaInfo.Model.AudioCodec.Wave, "WAVE" },
      { global::MediaInfo.Model.AudioCodec.Wave64, "WAVE" },
      { global::MediaInfo.Model.AudioCodec.Real14_4, "RA" },
      { global::MediaInfo.Model.AudioCodec.Real28_8, "RA" },
      { global::MediaInfo.Model.AudioCodec.Real10, "RA" },
      { global::MediaInfo.Model.AudioCodec.RealCook, "RA" },
      { global::MediaInfo.Model.AudioCodec.RealSipr, "RA" },
      { global::MediaInfo.Model.AudioCodec.RealRalf, "RA" },
      { global::MediaInfo.Model.AudioCodec.RealAtrc, "RA" },
      { global::MediaInfo.Model.AudioCodec.Mlp, "MLP" },
      { global::MediaInfo.Model.AudioCodec.Aac, "AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg2Main, "AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg2Lc, "AAC LC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg2LcSbr, "AAC HE-AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg2Ssr, "AAC HE-AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg4Main, "AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg4Lc, "AAC LC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg4LcSbr, "AAC HE-AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg4LcSbrPs, "AAC HE-AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg4Ssr, "AAC HE-AAC" },
      { global::MediaInfo.Model.AudioCodec.AacMpeg4Ltp, "AAC HE-AAC" },
      { global::MediaInfo.Model.AudioCodec.Alac, "ALAC" },
      { global::MediaInfo.Model.AudioCodec.Ape, "APE" },
      { global::MediaInfo.Model.AudioCodec.Wma1, "WMA" },
      { global::MediaInfo.Model.AudioCodec.Wma2, "WMA" },
      { global::MediaInfo.Model.AudioCodec.Wma3, "WMA" },
      { global::MediaInfo.Model.AudioCodec.WmaVoice, "WMA VOICE" },
      { global::MediaInfo.Model.AudioCodec.WmaPro, "WMAPRO" },
      { global::MediaInfo.Model.AudioCodec.WmaLossless, "WMA LOSSLESS" },
      { global::MediaInfo.Model.AudioCodec.Adpcm, "ADPCM" },
      { global::MediaInfo.Model.AudioCodec.Amr, "AMR" },
      { global::MediaInfo.Model.AudioCodec.Atrac1, "ATRAC" },
      { global::MediaInfo.Model.AudioCodec.Atrac3, "ATRAC3" },
      { global::MediaInfo.Model.AudioCodec.Atrac3Plus, "ATRAC3 PLUS" },
      { global::MediaInfo.Model.AudioCodec.AtracLossless, "ATRAC LOSSLESS" },
      { global::MediaInfo.Model.AudioCodec.Atrac9, "ATRAC9" },
      { global::MediaInfo.Model.AudioCodec.Dsd, "DSD" },
      { global::MediaInfo.Model.AudioCodec.Mac3, "MAC3" },
      { global::MediaInfo.Model.AudioCodec.Mac6, "MAC6" },
      { global::MediaInfo.Model.AudioCodec.G_723_1, "G.723.1" },
      { global::MediaInfo.Model.AudioCodec.Truespeech, "TRUESPEECH" },
      { global::MediaInfo.Model.AudioCodec.RkAudio, "RKAUDIO" },
      { global::MediaInfo.Model.AudioCodec.Als, "ALS" },
      { global::MediaInfo.Model.AudioCodec.Iac2, "IAC2" },
    };

    private static readonly Dictionary<global::MediaInfo.Model.VideoCodec, string> _videoCodecs = new Dictionary<global::MediaInfo.Model.VideoCodec, string>()
    {
      { global::MediaInfo.Model.VideoCodec.Undefined, string.Empty },
      { global::MediaInfo.Model.VideoCodec.Uncompressed, "RAW" },
      { global::MediaInfo.Model.VideoCodec.Dirac, "DIRAC" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4, "MPEG4" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4Is0Sp, "MPEG4" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4Is0Asp, "MPEG4VIDEO" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4Is0Ap, "MPEG4VIDEO" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4Is0Avc, "AVC" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4IsoSp, "MPEG4" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4IsoAsp, "MPEG4VIDEO" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4IsoAp, "MPEG4VIDEO" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4IsoAvc, "AVC" },
      { global::MediaInfo.Model.VideoCodec.MpeghIsoHevc, "HEVC" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4MsV1, "MSMPEG4V1" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4MsV2, "MSMPEG4V2" },
      { global::MediaInfo.Model.VideoCodec.Mpeg4MsV3, "MSMPEG4V3" },
      { global::MediaInfo.Model.VideoCodec.Vc1, "VC-1" },
      { global::MediaInfo.Model.VideoCodec.Mpeg1, "MPEG1VIDEO" },
      { global::MediaInfo.Model.VideoCodec.Mpeg2, "MPEG2VIDEO" },
      { global::MediaInfo.Model.VideoCodec.ProRes, "PRORES" },
      { global::MediaInfo.Model.VideoCodec.RealRv10, "REAL" },
      { global::MediaInfo.Model.VideoCodec.RealRv20, "REAL" },
      { global::MediaInfo.Model.VideoCodec.RealRv30, "RV30" },
      { global::MediaInfo.Model.VideoCodec.RealRv40, "RV40" },
      { global::MediaInfo.Model.VideoCodec.Theora, "THEORA" },
      { global::MediaInfo.Model.VideoCodec.Vp6, "VP6F" },
      { global::MediaInfo.Model.VideoCodec.Vp8, "VP8" },
      { global::MediaInfo.Model.VideoCodec.Vp9, "VP9" },
      { global::MediaInfo.Model.VideoCodec.Divx1, "DIVX" },
      { global::MediaInfo.Model.VideoCodec.Divx2, "DIV2" },
      { global::MediaInfo.Model.VideoCodec.Divx3, "DIV3" },
      { global::MediaInfo.Model.VideoCodec.Divx4, "DIVX 4" },
      { global::MediaInfo.Model.VideoCodec.Divx50, "DX50" },
      { global::MediaInfo.Model.VideoCodec.Xvid, "XVID" },
      { global::MediaInfo.Model.VideoCodec.Svq1, "SVQ1" },
      { global::MediaInfo.Model.VideoCodec.Svq2, "SVQ2" },
      { global::MediaInfo.Model.VideoCodec.Svq3, "SVQ3" },
      { global::MediaInfo.Model.VideoCodec.Sprk, "FLASH" },
      { global::MediaInfo.Model.VideoCodec.H260, "H260" },
      { global::MediaInfo.Model.VideoCodec.H261, "H261" },
      { global::MediaInfo.Model.VideoCodec.H263, "H263" },
      { global::MediaInfo.Model.VideoCodec.Avdv, "AVDV" },
      { global::MediaInfo.Model.VideoCodec.Avd1, "AVD1" },
      { global::MediaInfo.Model.VideoCodec.Ffv1, "FFV1" },
      { global::MediaInfo.Model.VideoCodec.Ffv2, "FFV2" },
      { global::MediaInfo.Model.VideoCodec.Iv21, "IV21" },
      { global::MediaInfo.Model.VideoCodec.Iv30, "IV30" },
      { global::MediaInfo.Model.VideoCodec.Iv40, "IV40" },
      { global::MediaInfo.Model.VideoCodec.Iv50, "IV50" },
      { global::MediaInfo.Model.VideoCodec.Ffds, "MPEG4" },
      { global::MediaInfo.Model.VideoCodec.Fraps, "MPEG4" },
      { global::MediaInfo.Model.VideoCodec.Ffvh, "MPEG4" },
      { global::MediaInfo.Model.VideoCodec.Mjpg, "MJPG" },
      { global::MediaInfo.Model.VideoCodec.Dv, "DV" },
      { global::MediaInfo.Model.VideoCodec.Hdv, "HDV" },
      { global::MediaInfo.Model.VideoCodec.DvcPro50, "DVCPRO50" },
      { global::MediaInfo.Model.VideoCodec.DvcProHd, "DVCPROHD" },
      { global::MediaInfo.Model.VideoCodec.Wmv1, "WMV" },
      { global::MediaInfo.Model.VideoCodec.Wmv2, "WMV2" },
      { global::MediaInfo.Model.VideoCodec.Wmv3, "WMV3" },
      { global::MediaInfo.Model.VideoCodec.Q8Bps, "Q8BPS" },
      { global::MediaInfo.Model.VideoCodec.BinkVideo, "BINKVIDEO" },
      { global::MediaInfo.Model.VideoCodec.Av1, "AV1" },
      { global::MediaInfo.Model.VideoCodec.HuffYUV, "HUFFYUV" },
    };

    private static readonly Dictionary<global::MediaInfo.Model.AspectRatio, string> _aspectRatios = new Dictionary<global::MediaInfo.Model.AspectRatio, string>
    {
      { global::MediaInfo.Model.AspectRatio.Opaque, "1:1"},
      { global::MediaInfo.Model.AspectRatio.HighEndDataGraphics, "5:4"},
      { global::MediaInfo.Model.AspectRatio.StandardSlides, "3:3"},
      { global::MediaInfo.Model.AspectRatio.FullScreen, "4:3"},
      { global::MediaInfo.Model.AspectRatio.DigitalSlrCameras, "3:2"},
      { global::MediaInfo.Model.AspectRatio.HighDefinitionTv, "16:9"},
      { global::MediaInfo.Model.AspectRatio.WideScreenDisplay, "16:10"},
      { global::MediaInfo.Model.AspectRatio.WideScreen, "1.85:1"},
      { global::MediaInfo.Model.AspectRatio.CinemaScope, "21:9"},
    };

    private static readonly Dictionary<global::MediaInfo.Model.AspectRatio, double> _aspectRatiosValue = new Dictionary<global::MediaInfo.Model.AspectRatio, double>
    {
      { global::MediaInfo.Model.AspectRatio.Opaque, 1},
      { global::MediaInfo.Model.AspectRatio.HighEndDataGraphics, 1.25},
      { global::MediaInfo.Model.AspectRatio.StandardSlides, 1},
      { global::MediaInfo.Model.AspectRatio.FullScreen, 1.33},
      { global::MediaInfo.Model.AspectRatio.DigitalSlrCameras, 1.5},
      { global::MediaInfo.Model.AspectRatio.HighDefinitionTv, 1.77},
      { global::MediaInfo.Model.AspectRatio.WideScreenDisplay, 1.6},
      { global::MediaInfo.Model.AspectRatio.WideScreen, 1.85},
      { global::MediaInfo.Model.AspectRatio.CinemaScope, 2.33},
    };

    #endregion

    public static string ToCodecString(this global::MediaInfo.Model.AudioCodec codec)
    {
      string result;
      return _audioCodecs.TryGetValue(codec, out result) ? result : string.Empty;
    }

    public static string ToCodecString(this global::MediaInfo.Model.VideoCodec codec)
    {
      string result;
      return _videoCodecs.TryGetValue(codec, out result) ? result : string.Empty;
    }

    public static string ToAspectRatioString(this global::MediaInfo.Model.AspectRatio ratio)
    {
      string result;
      return _aspectRatios.TryGetValue(ratio, out result) ? result : string.Empty;
    }

    public static double ToAspectRatioValue(this global::MediaInfo.Model.AspectRatio ratio)
    {
      double result;
      return _aspectRatiosValue.TryGetValue(ratio, out result) ? result : 1.77;
    }
  }
}
