import subprocess
import os
import glob

EXE_PATH = r".\GbxMedalTimeModifier.exe"

# I do not recommend having these be the same location...
# Please also make sure to change this location to the correct location; where your maps are located.
INPUT_DIR  = r"C:\Users\ar\Documents\Trackmania2020\Maps\My Maps"
OUTPUT_DIR = r"C:\Users\ar\Documents\Trackmania2020\Maps\My Maps\Dings"

AT_MS     = str(3600_000)   # 1 hour
GOLD_MS   = str(7200_000)   # 2 hours
SILVER_MS = str(10800_000)  # 3 hours
BRONZE_MS = str(14400_000)  # 4 hours

os.makedirs(OUTPUT_DIR, exist_ok=True)

for input_path in glob.glob(os.path.join(INPUT_DIR, "*.Gbx")):
    filename   = os.path.basename(input_path)
    output_path = os.path.join(OUTPUT_DIR, filename)
    cmd = [
        EXE_PATH,
        input_path,
        output_path,
        AT_MS,
        GOLD_MS,
        SILVER_MS,
        BRONZE_MS
    ]
    print(f"Running: {' '.join(cmd)}")
    try:
        subprocess.run(cmd, check=True)
        print(f"Successfully processed {filename}\n")
    except subprocess.CalledProcessError as e:
        print(f"Error processing {filename}: {e}\n")