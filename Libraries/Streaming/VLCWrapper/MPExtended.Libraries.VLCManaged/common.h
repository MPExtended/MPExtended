// Copyright (C) 2011 MPExtended Developers, http://mpextended.github.com/
// 
// MPExtended is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MPExtended is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MPExtended. If not, see <http://www.gnu.org/licenses/>.
//
// This is a wrapper around some streaming functionality of vlc, to also
// make it available in C#, where our main program is written.
//
// Usage of libvlc on Windows:
// - You need a full VLC installation with the sdk folder from the
//   standalone .zip file.
// - Create .lib file (http://wiki.videolan.org/GenerateLibFromDll)
// - Add sdk/include to the include directory
// - Add the directory with the .lib file to the library directory
// - Add the .lib file as an additional library

#ifndef _common_h
#define _common_h

#define VERSION "0.1.0.0"

#define USER_AGENT "VLC Wrapper for MPExtended"
#define HTTP_USER_AGENT "MPExtended VLCManaged/0.1.0"

// #define DEBUG_CPPCLI_CONSOLE 1

#ifdef DEBUG_CPPCLI_CONSOLE
	#define DEBUG(...) System::Console::WriteLine(__VA_ARGS__);
#else
	#define DEBUG(...)
#endif

#define gcstring(x) gcnew System::String(x)

#endif