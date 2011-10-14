// Copyright (C) 2011 MPExtended Developers, http://mpextended.codeplex.com/
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

#include "VLCTranscoder.h"

using namespace MPExtended::Libraries::VLCManaged;
using namespace System::Runtime::InteropServices;

VLCTranscoder::VLCTranscoder() {
	vlc = new VLCNative;
}

// TODO: we leak some bytes here
void VLCTranscoder::SetArguments(array<String^>^ args) {
	int count = args->Length;
	char **list = new char*[count];
	int i = 0;
	for each(String^% s in args) {
		IntPtr p = Marshal::StringToHGlobalAnsi(s);
		list[i] = static_cast<char*>(p.ToPointer());
		i++;
	}
	vlc->setArguments(count, list);
}

// TODO: we leak some bytes here
void VLCTranscoder::SetInput(String ^ input) {
	IntPtr p = Marshal::StringToHGlobalAnsi(input);
   char *chars = static_cast<char*>(p.ToPointer());
	vlc->setInput(chars);
}

// TODO: we leak some bytes here
void VLCTranscoder::SetSout(String ^ sout) {
	IntPtr p = Marshal::StringToHGlobalAnsi(sout);
   char *chars = static_cast<char*>(p.ToPointer());
	vlc->setSout(chars);
}

// TODO: we leak some bytes here
void VLCTranscoder::SetMediaName(String ^ sout) {
	IntPtr p = Marshal::StringToHGlobalAnsi(sout);
   char *chars = static_cast<char*>(p.ToPointer());
	vlc->setMediaName(chars);
}

void VLCTranscoder::StartTranscoding() {
	vlc->startTranscoding();
}

void VLCTranscoder::Seek(float position) {
	vlc->seek(position);
}

float VLCTranscoder::GetPosition() {
	return vlc->getPosition();
}

int VLCTranscoder::GetTime() {
	return vlc->getTime();
}

void VLCTranscoder::StopTranscoding() {
	vlc->stopTranscoding();
}