import sys
import os
import hashlib

importFile = False

def XOR_All_Bytes(romBytes: bytes, ghidraBytes: bytes):
    xorBytes = bytearray(len(ghidraBytes))
    xorBytes[:] = ghidraBytes
    for i in range(len(ghidraBytes)):
        xorBytes[i] ^= romBytes[i % len(romBytes)]
    return bytes(xorBytes)


args = sys.argv[1:]
print()

# Check for exactly 4 arguments
if (len(args) != 4):
    print("Error: Need exactly 4 arguments (don't forget quotes around file paths if they have spaces):")
    print("1: import (cgzf->gzf)  or export (gzf->cgzf)")
    print("2: GZF/CGZF file")
    print("3: ROM binary location")
    print("4: Output folder")
    print()
    print("Got " + str(len(args)) + " arguments:")
    print(args)
    quit()

# Check first argument is either import or export
if (args[0].lower() == "import"):
    importFile = True
elif (args[0].lower() == "export"):
    importFile = False
else:
    print("Error: First argument must either be import (cgzf->gzf) or export (gzf->cgzf)")
    quit()

# Check for minimum file name length
if ((importFile and len(args[1]) < 6) or (not importFile and len(args[1]) < 5)):
    print("Error: Second arg (GZF/CGZF file) is not a valid file (be sure to include the extension)")
    quit()

# Check for correct file type
if ((importFile and args[1][-5:] != ".cgzf")) or (not importFile and args[1][-4:] != ".gzf"):
    print("Error: Second arg (GZF/CGZF file) must be of filetype .cgzf for import, filetype .gzf for export")
    quit()

# Check for output location
if (not os.path.exists(args[3])):
    print("Error: Output directory does not exist")
    quit()

# Try to load GZF/CGZF
try:
    with open(args[1], 'rb') as file:
        cgzfBytes = file.read()
        file.close()
except:
    print("Couldn't load GZF/CGZF (is the path correct?)")

# Try to load ROM
try:
    with open(args[2], 'rb') as file:
        romBytes = file.read()
        file.close()
except:
    print("Couldn't load ROM (is the path correct?)")

if (importFile):
    # Don't decode the last 16 bytes which make the checksum
    outBytes = XOR_All_Bytes(romBytes, cgzfBytes[0:-16])
    retrievedHash = cgzfBytes[-16:]

    calculatedHash = hashlib.md5(outBytes).digest()
    if (calculatedHash != retrievedHash):
        print("Error: File checksum does not match expected. \n\nThis likely means the ROM used does not match the one used to generate the .cgzf")
        quit()

    path = args[3] + "/" + os.path.basename(args[1])[0:-5] + "_DECODE.gzf"
else:
    hash = hashlib.md5(cgzfBytes).digest()
    xorBytes = XOR_All_Bytes(romBytes, cgzfBytes)
    outBytes = xorBytes + hash
    path = args[3] + "/" + os.path.basename(args[1])[0:-4] + "_CLEAN.cgzf"

# Save file
try:
    if (os.path.exists(path)):
        os.remove(path)

    with open(path, 'wb') as file:
        # Write the bytes to the file
        file.write(outBytes)
        file.close()
        print("Conversion success!")
        print("Output file: " + path)
except:
    print("Error: Couldn't save converted file")