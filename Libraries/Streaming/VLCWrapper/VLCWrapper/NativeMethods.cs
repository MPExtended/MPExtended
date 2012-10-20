//    LibVLC Interop
//    based on the nVLC converted headers by Roman Ginzburg

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MPExtended.Libraries.VLCWrapper
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct libvlc_event_t
    {
        internal libvlc_event_e type;
        internal IntPtr p_obj;
        internal MediaDescriptorUnion MediaDescriptor;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_meta_changed
    {
        internal libvlc_meta_t meta_type;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_subitem_added
    {
        internal IntPtr new_child;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_duration_changed
    {
        internal long new_duration;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_parsed_changed
    {
        internal int new_status;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_freed
    {
        internal IntPtr md;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_state_changed
    {
        internal libvlc_state_t new_state;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_position_changed
    {
        internal float new_position;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_time_changed
    {
        internal long new_time;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_title_changed
    {
        internal int new_title;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_seekable_changed
    {
        internal int new_seekable;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_pausable_changed
    {
        internal int new_pausable;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_list_item_added
    {
        internal IntPtr item;
        internal int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_list_will_add_item
    {
        internal IntPtr item;
        internal int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_list_item_deleted
    {
        internal IntPtr item;
        internal int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_list_will_delete_item
    {
        internal IntPtr item;
        internal int index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_list_player_next_item_set
    {
        internal IntPtr item;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_snapshot_taken
    {
        internal IntPtr psz_filename;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_length_changed
    {
        internal long new_length;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct vlm_media_event
    {
        internal IntPtr psz_media_name;
        internal IntPtr psz_instance_name;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct media_player_media_changed
    {
        internal IntPtr new_media;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MediaDescriptorUnion
    {
        [FieldOffset(0)]
        internal media_meta_changed media_meta_changed;

        [FieldOffset(0)]
        internal media_subitem_added media_subitem_added;

        [FieldOffset(0)]
        internal media_duration_changed media_duration_changed;

        [FieldOffset(0)]
        internal media_parsed_changed media_parsed_changed;

        [FieldOffset(0)]
        internal media_freed media_freed;

        [FieldOffset(0)]
        internal media_state_changed media_state_changed;

        [FieldOffset(0)]
        internal media_player_position_changed media_player_position_changed;

        [FieldOffset(0)]
        internal media_player_time_changed media_player_time_changed;

        [FieldOffset(0)]
        internal media_player_title_changed media_player_title_changed;

        [FieldOffset(0)]
        internal media_player_seekable_changed media_player_seekable_changed;

        [FieldOffset(0)]
        internal media_player_pausable_changed media_player_pausable_changed;

        [FieldOffset(0)]
        internal media_list_item_added media_list_item_added;

        [FieldOffset(0)]
        internal media_list_will_add_item media_list_will_add_item;

        [FieldOffset(0)]
        internal media_list_item_deleted media_list_item_deleted;

        [FieldOffset(0)]
        internal media_list_will_delete_item media_list_will_delete_item;

        [FieldOffset(0)]
        internal media_list_player_next_item_set media_list_player_next_item_set;

        [FieldOffset(0)]
        internal media_player_snapshot_taken media_player_snapshot_taken;

        [FieldOffset(0)]
        internal media_player_length_changed media_player_length_changed;

        [FieldOffset(0)]
        internal vlm_media_event vlm_media_event;

        [FieldOffset(0)]
        internal media_player_media_changed media_player_media_changed;
    }

    internal enum libvlc_state_t
    {
        libvlc_NothingSpecial = 0,
        libvlc_Opening,
        libvlc_Buffering,
        libvlc_Playing,
        libvlc_Paused,
        libvlc_Stopped,
        libvlc_Ended,
        libvlc_Error
    }

    internal enum libvlc_event_e
    {
        libvlc_MediaMetaChanged = 0,
        libvlc_MediaSubItemAdded,
        libvlc_MediaDurationChanged,
        libvlc_MediaParsedChanged,
        libvlc_MediaFreed,
        libvlc_MediaStateChanged,

        libvlc_MediaPlayerMediaChanged = 0x100,
        libvlc_MediaPlayerNothingSpecial,
        libvlc_MediaPlayerOpening,
        libvlc_MediaPlayerBuffering,
        libvlc_MediaPlayerPlaying,
        libvlc_MediaPlayerPaused,
        libvlc_MediaPlayerStopped,
        libvlc_MediaPlayerForward,
        libvlc_MediaPlayerBackward,
        libvlc_MediaPlayerEndReached,
        libvlc_MediaPlayerEncounteredError,
        libvlc_MediaPlayerTimeChanged,
        libvlc_MediaPlayerPositionChanged,
        libvlc_MediaPlayerSeekableChanged,
        libvlc_MediaPlayerPausableChanged,
        libvlc_MediaPlayerTitleChanged,
        libvlc_MediaPlayerSnapshotTaken,
        libvlc_MediaPlayerLengthChanged,

        libvlc_MediaListItemAdded = 0x200,
        libvlc_MediaListWillAddItem,
        libvlc_MediaListItemDeleted,
        libvlc_MediaListWillDeleteItem,

        libvlc_MediaListViewItemAdded = 0x300,
        libvlc_MediaListViewWillAddItem,
        libvlc_MediaListViewItemDeleted,
        libvlc_MediaListViewWillDeleteItem,

        libvlc_MediaListPlayerPlayed = 0x400,
        libvlc_MediaListPlayerNextItemSet,
        libvlc_MediaListPlayerStopped,

        libvlc_MediaDiscovererStarted = 0x500,
        libvlc_MediaDiscovererEnded,

        libvlc_VlmMediaAdded = 0x600,
        libvlc_VlmMediaRemoved,
        libvlc_VlmMediaChanged,
        libvlc_VlmMediaInstanceStarted,
        libvlc_VlmMediaInstanceStopped,
        libvlc_VlmMediaInstanceStatusInit,
        libvlc_VlmMediaInstanceStatusOpening,
        libvlc_VlmMediaInstanceStatusPlaying,
        libvlc_VlmMediaInstanceStatusPause,
        libvlc_VlmMediaInstanceStatusEnd,
        libvlc_VlmMediaInstanceStatusError,
    }

    internal enum libvlc_meta_t
    {
        libvlc_meta_Title,
        libvlc_meta_Artist,
        libvlc_meta_Genre,
        libvlc_meta_Copyright,
        libvlc_meta_Album,
        libvlc_meta_TrackNumber,
        libvlc_meta_Description,
        libvlc_meta_Rating,
        libvlc_meta_Date,
        libvlc_meta_Setting,
        libvlc_meta_URL,
        libvlc_meta_Language,
        libvlc_meta_NowPlaying,
        libvlc_meta_Publisher,
        libvlc_meta_EncodedBy,
        libvlc_meta_ArtworkURL,
        libvlc_meta_TrackID
    }

    internal class NativeMethods
    {
        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern int libvlc_event_attach(IntPtr p_event_manager, libvlc_event_e i_event_type, IntPtr f_callback, IntPtr user_data);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_get_version();

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_new(int argc, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] argv);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_set_user_agent(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] name, [MarshalAs(UnmanagedType.LPArray)] byte[] http);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_new_path(IntPtr p_instance, [MarshalAs(UnmanagedType.LPArray)] byte[] psz_mrl);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_add_option(IntPtr libvlc_media_inst, [MarshalAs(UnmanagedType.LPArray)] byte[] ppsz_options);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_new_from_media(IntPtr libvlc_media);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr libvlc_media_player_event_manager(IntPtr libvlc_media_player_t);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_play(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern Int64 libvlc_media_player_get_time(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_release(IntPtr libvlc_media_inst);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_release(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_media_player_stop(IntPtr libvlc_mediaplayer);

        [DllImport("libvlc", CallingConvention = CallingConvention.Cdecl)]
        public static extern void libvlc_release(IntPtr libvlc_instance_t);
    }
}
