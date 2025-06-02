import sys
import os
import hashlib
from Crypto.Cipher import AES
from Crypto.Util.Padding import pad, unpad
import secrets

importFile = False
versionMajor = 2
versionMinor = 0
versionInfoLen = 2

saltLen = 16

forceVersion = False
headerIDStr = "CGZF"

def XOR_All_Bytes(romBytes: bytes, ghidraBytes: bytes):
    xorBytes = bytearray(len(ghidraBytes))
    xorBytes[:] = ghidraBytes
    for i in range(len(ghidraBytes)):
        xorBytes[i] ^= romBytes[i % len(romBytes)]
    return bytes(xorBytes)


def Get_Key_Info(romBytes: bytes, salt: bytes):
    inputBytes = romBytes + salt
    hash_object = hashlib.sha512()
    hash_object.update(inputBytes)
    return hash_object.digest()


def Encrypt_With_ROM(romBytes: bytes, ghidraBytes: bytes, salt: bytes):
    keyInfo = Get_Key_Info(romBytes, salt)
    key = keyInfo[32:]
    iv = keyInfo[:16]
    
    cipher = AES.new(key, AES.MODE_CBC, iv)
    paddedBytes = pad(ghidraBytes, AES.block_size)
    #paddedBytes = ghidraBytes + bytes([0xFF]) * (AES.block_size - (len(ghidraBytes) % AES.block_size))
    encryptedBytes = cipher.encrypt(paddedBytes)

    return encryptedBytes + salt


def Decrypt_With_ROM(romBytes: bytes, inBytes: bytes, saltLen: int):
    salt = inBytes[-saltLen:]
    encryptedBytes = inBytes[:-saltLen]

    keyInfo = Get_Key_Info(romBytes, salt)
    key = keyInfo[32:]
    iv = keyInfo[:16]

    cipher = AES.new(key, AES.MODE_CBC, iv)
    decryptedBytes = unpad(cipher.decrypt(encryptedBytes), AES.block_size)

    return decryptedBytes


args = sys.argv[1:]
print()
print("GhidraZipCleaner version " + str(versionMajor) + "." + str(versionMinor))
print()

# Check for exactly 4 arguments
if (len(args) != 4):
    if (len(args) == 5):
        try:
            versionMajor = int(args[4])
            forceVersion = True
            if (versionMajor == 1):
                print("WARNING!!!  Forcing legacy mode, which can generate cgzf's that are not copyright safe.");
                print("This should only be used to import old cgzf's generated with the old version of GhidraZipCleaner.");
                print()
            else:
                print("Using version: " + versionMajor.ToString())
        except:
            print("Error: Need exactly 4 arguments (don't forget quotes around file paths if they have spaces):")
            print("1: import (cgzf->gzf)  or export (gzf->cgzf)")
            print("2: GZF/CGZF file")
            print("3: ROM binary location")
            print("4: Output folder")
            print()
            print("Got " + str(len(args)) + " arguments:")
            print(args)
            quit()
    else:
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
    print("Error: Couldn't load GZF/CGZF (is the path correct?)")
    quit()

# Try to load ROM
try:
    with open(args[2], 'rb') as file:
        romBytes = file.read()
        file.close()
except:
    print("Error: Couldn't load ROM (is the path correct?)")
    quit()

if (importFile):
    # Importing file
    if (not forceVersion):
        # Check CGZF version
        headerStrFound = True
        for i in range(len(headerIDStr)):
            if (cgzfBytes[i] != int(ord(headerIDStr[i]))):
                headerStrFound = False
                break

        if (headerStrFound):
            versionMajor = cgzfBytes[len(headerIDStr)]
            versionMinor = cgzfBytes[len(headerIDStr)+1]
        else:
            # No header ID found, meaning legacy edition
            versionMajor = 1
            versionMinor = 0

    retrievedHash = cgzfBytes[-16:]

    # Don't decode the last 16 bytes which make the checksum
    if (versionMajor == 1):
        print("Importing with legacy mode...")
        outBytes = XOR_All_Bytes(romBytes, cgzfBytes[0:-16])
        calculatedHash = hashlib.md5(outBytes).digest()
    else:
        outBytes = Decrypt_With_ROM(romBytes, cgzfBytes[len(headerIDStr)+versionInfoLen:-16], saltLen)
        calculatedHash = hashlib.md5(outBytes[:400]).digest()
        print("Importing from version: " + str(versionMajor) + "." + str(versionMinor))
        print()

    if (calculatedHash != retrievedHash):
        print("Error: File checksum does not match expected. \n\nThis likely means the ROM used does not match the one used to generate the .cgzf")
        quit()

    path = args[3] + "/" + os.path.basename(args[1])[0:-5] + "_DECODE.gzf"
else:
    # Exporting file
    # Generate random salt
    salt = secrets.token_bytes(saltLen)

    if (versionMajor == 1):
        hash = hashlib.md5(cgzfBytes).digest()
        xorBytes = XOR_All_Bytes(romBytes, cgzfBytes)
        outBytes = xorBytes + hash
    else:
        hash = hashlib.md5(cgzfBytes[:400]).digest()
        encryptedBytes = Encrypt_With_ROM(romBytes, cgzfBytes, salt)
        outBytes = headerIDStr.encode('utf-8') + bytes([versionMajor, versionMinor]) + encryptedBytes + hash
        print("Exporting with version: " + str(versionMajor) + "." + str(versionMinor))
        print()

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